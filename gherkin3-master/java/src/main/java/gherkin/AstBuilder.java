package gherkin;

import gherkin.ast.Background;
import gherkin.ast.DataTable;
import gherkin.ast.DocString;
import gherkin.ast.Examples;
import gherkin.ast.Feature;
import gherkin.ast.Location;
import gherkin.ast.Scenario;
import gherkin.ast.ScenarioDefinition;
import gherkin.ast.ScenarioOutline;
import gherkin.ast.Step;
import gherkin.ast.StepArgument;
import gherkin.ast.TableCell;
import gherkin.ast.TableRow;
import gherkin.ast.Tag;

import java.util.ArrayDeque;
import java.util.ArrayList;
import java.util.Deque;
import java.util.List;

import static gherkin.Parser.IAstBuilder;
import static gherkin.Parser.RuleType;
import static gherkin.Parser.TokenType;
import static gherkin.StringUtils.join;

public class AstBuilder<T> implements IAstBuilder<T> {
    private final Deque<AstNode> stack = new ArrayDeque<>();

    public AstBuilder() {
        stack.push(new AstNode(RuleType.None));
    }

    private AstNode currentNode() {
        return stack.peek();
    }

    @Override
    public void build(Token token) {
        RuleType ruleType = RuleType.cast(token.matchedType);
        currentNode().add(ruleType, token);
    }

    @Override
    public void startRule(RuleType ruleType) {
        stack.push(new AstNode(ruleType));
    }

    @Override
    public void endRule(RuleType ruleType) {
        AstNode node = stack.pop();
        Object transformedNode = getTransformedNode(node);
        currentNode().add(node.ruleType, transformedNode);
    }

    private Object getTransformedNode(AstNode node) {
        switch (node.ruleType) {
            case Step: {
                Token stepLine = node.getToken(TokenType.StepLine);
                StepArgument stepArg = node.getSingle(RuleType.DataTable, null);
                if (stepArg == null) {
                    stepArg = node.getSingle(RuleType.DocString, null);
                }
                return new Step(getLocation(stepLine, 0), stepLine.matchedKeyword, stepLine.matchedText, stepArg);
            }
            case DocString: {
                Token separatorToken = node.getTokens(TokenType.DocStringSeparator).get(0);
                String contentType = separatorToken.matchedText;
                List<Token> lineTokens = node.getTokens(TokenType.Other);
                StringBuilder content = new StringBuilder();
                boolean newLine = false;
                for (Token lineToken : lineTokens) {
                    if (newLine) content.append("\n");
                    newLine = true;
                    content.append(lineToken.matchedText);
                }
                return new DocString(getLocation(separatorToken, 0), contentType, content.toString());
            }
            case DataTable: {
                List<TableRow> rows = getTableRows(node);
                return new DataTable(rows);
            }
            case Background: {
                Token backgroundLine = node.getToken(TokenType.BackgroundLine);
                String description = getDescription(node);
                List<Step> steps = getSteps(node);
                return new Background(getLocation(backgroundLine, 0), backgroundLine.matchedKeyword, backgroundLine.matchedText, description, steps);
            }
            case Scenario_Definition: {
                List<Tag> tags = getTags(node);
                AstNode scenarioNode = node.getSingle(RuleType.Scenario, null);

                if (scenarioNode != null) {
                    Token scenarioLine = scenarioNode.getToken(TokenType.ScenarioLine);
                    String description = getDescription(scenarioNode);
                    List<Step> steps = getSteps(scenarioNode);

                    return new Scenario(tags, getLocation(scenarioLine, 0), scenarioLine.matchedKeyword, scenarioLine.matchedText, description, steps);
                } else {
                    AstNode scenarioOutlineNode = node.getSingle(RuleType.ScenarioOutline, null);
                    if (scenarioOutlineNode == null) {
                        throw new RuntimeException("Internal grammar error");
                    }
                    Token scenarioOutlineLine = scenarioOutlineNode.getToken(TokenType.ScenarioOutlineLine);
                    String description = getDescription(scenarioOutlineNode);
                    List<Step> steps = getSteps(scenarioOutlineNode);

                    List<Examples> examplesList = scenarioOutlineNode.getItems(RuleType.Examples);

                    return new ScenarioOutline(tags, getLocation(scenarioOutlineLine, 0), scenarioOutlineLine.matchedKeyword, scenarioOutlineLine.matchedText, description, steps, examplesList);

                }
            }
            case Examples: {
                List<Tag> tags = getTags(node);
                Token examplesLine = node.getToken(TokenType.ExamplesLine);
                String description = getDescription(node);
                List<TableRow> allRows = getTableRows(node);
                TableRow header = allRows.get(0);
                List<TableRow> rows = allRows.subList(1, allRows.size());
                return new Examples(getLocation(examplesLine, 0), tags, examplesLine.matchedKeyword, examplesLine.matchedText, description, header, rows);
            }
            case Description: {
                List<Token> lineTokens = node.getTokens(TokenType.Other);
                // Trim trailing empty lines
                int end = lineTokens.size();
                while (end > 0 && lineTokens.get(end - 1).matchedText.matches("\\s*")) {
                    end--;
                }
                lineTokens = lineTokens.subList(0, end);

                return join(new StringUtils.ToString<Token>() {
                    @Override
                    public String toString(Token t) {
                        return t.matchedText;
                    }
                }, "\n", lineTokens);
            }
            case Feature: {
                AstNode header = node.getSingle(RuleType.Feature_Header, new AstNode(RuleType.Feature_Header));
                List<Tag> tags = getTags(header);
                Token featureLine = header.getToken(TokenType.FeatureLine);
                Background background = node.getSingle(RuleType.Background, null);
                List<ScenarioDefinition> scenarioDefinitions = node.getItems(RuleType.Scenario_Definition);
                String description = getDescription(header);
                String language = featureLine.matchedGherkinDialect.getLanguage();

                return new Feature(tags, getLocation(featureLine, 0), language, featureLine.matchedKeyword, featureLine.matchedText, description, background, scenarioDefinitions);
            }

        }
        return node;
    }

    private List<TableRow> getTableRows(AstNode node) {
        List<TableRow> rows = new ArrayList<>();
        for (Token token : node.getTokens(TokenType.TableRow)) {
            rows.add(new TableRow(getLocation(token, 0), getCells(token)));
        }
        ensureCellCount(rows);
        return rows;
    }

    private void ensureCellCount(List<TableRow> rows) {
        if (rows.isEmpty()) return;

        int cellCount = rows.get(0).getCells().size();
        for (TableRow row : rows) {
            if (row.getCells().size() != cellCount) {
                throw new ParserException.AstBuilderException("inconsistent cell count within the table", row.getLocation());
            }
        }
    }

    private List<TableCell> getCells(Token token) {
        List<TableCell> cells = new ArrayList<>();
        for (GherkinLineSpan cellItem : token.mathcedItems) {
            cells.add(new TableCell(getLocation(token, cellItem.Column), cellItem.Text));
        }
        return cells;
    }

    private List<Step> getSteps(AstNode node) {
        return node.getItems(RuleType.Step);
    }

    private Location getLocation(Token token, int column) {
        return column == 0 ? token.location : new Location(token.location.getLine(), column);
    }

    private String getDescription(AstNode node) {
        return node.getSingle(RuleType.Description, null);
    }

    private List<Tag> getTags(AstNode node) {
        AstNode tagsNode = node.getSingle(RuleType.Tags, new AstNode(RuleType.None));
        if (tagsNode == null)
            return new ArrayList<>();

        List<Token> tokens = tagsNode.getTokens(TokenType.TagLine);
        List<Tag> tags = new ArrayList<>();
        for (Token token : tokens) {
            for (GherkinLineSpan tagItem : token.mathcedItems) {
                tags.add(new Tag(getLocation(token, tagItem.Column), tagItem.Text));
            }
        }
        return tags;
    }

    @Override
    public T getResult() {
        return currentNode().getSingle(RuleType.Feature, null);
    }
}
