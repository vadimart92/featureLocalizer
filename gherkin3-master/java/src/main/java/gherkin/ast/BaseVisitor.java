package gherkin.ast;

public class BaseVisitor implements Visitor {
    @Override
    public void visitFeature(Feature feature) {
        throw new UnsupportedOperationException();
    }

    @Override
    public void visitBackground(Background background) {
        throw new UnsupportedOperationException();
    }

    @Override
    public void visitScenario(Scenario scenario) {
        throw new UnsupportedOperationException();
    }

    @Override
    public void visitScenarioOutline(ScenarioOutline scenarioOutline) {
        throw new UnsupportedOperationException();
    }

    @Override
    public void visitExamples(Examples examples) {
        throw new UnsupportedOperationException();
    }

    @Override
    public void visitStep(Step step) {
        throw new UnsupportedOperationException();
    }
}
