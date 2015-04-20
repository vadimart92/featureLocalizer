GOOD_FEATURE_FILES = $(shell find ../testdata/good -name "*.feature")
BAD_FEATURE_FILES  = $(shell find ../testdata/bad -name "*.feature")

TOKENS   = $(patsubst ../testdata/%.feature,acceptance/testdata/%.feature.tokens,$(GOOD_FEATURE_FILES))
AST      = $(patsubst ../testdata/%.feature,acceptance/testdata/%.feature.ast,$(GOOD_FEATURE_FILES))
ERRORS   = $(patsubst ../testdata/%.feature,acceptance/testdata/%.feature.errors,$(BAD_FEATURE_FILES))

CS_FILES = $(shell find . -type f \( -iname "*.cs" ! -iname "*.NETFramework*" \))
NUNIT = packages/NUnit.Runners.2.6.3/tools/nunit-console.exe

all: .compared
.PHONY: all

.compared: .built $(TOKENS) $(AST) $(ERRORS)
	touch $@

.built: .sln_built_debug $(NUNIT) dialects.json
	mono --runtime=v4.0 $(NUNIT) -noxml -nologo -stoponerror Gherkin/bin/Debug/Gherkin.dll
	touch $@

acceptance/testdata/%.feature.tokens: ../testdata/%.feature ../testdata/%.feature.tokens .built
	mkdir -p `dirname $@`
	bin/gherkin-generate-tokens $< > $@
	diff --unified $<.tokens $@
.DELETE_ON_ERROR: acceptance/testdata/%.feature.tokens

acceptance/testdata/%.feature.ast: ../testdata/%.feature ../testdata/%.feature.ast .built
	mkdir -p `dirname $@`
	bin/gherkin-generate-ast-text $< > $@
	diff --unified $<.ast $@
.DELETE_ON_ERROR: acceptance/testdata/%.feature.ast

acceptance/testdata/%.feature.errors: ../testdata/%.feature ../testdata/%.feature.errors .built
	mkdir -p `dirname $@`
	! bin/gherkin-generate-ast-text $< > $@
	diff --unified $<.errors $@
.DELETE_ON_ERROR: acceptance/testdata/%.feature.errors

clean:
	rm -rf .compared .built acceptance
	rm -rf */bin
	rm -rf */obj
	rm -rf */packages
	rm -f Gherkin/Parser.cs
.PHONY: clean

Gherkin/Parser.cs: ../gherkin.berp gherkin-csharp.razor ../bin/berp.exe
	mono ../bin/berp.exe -g ../gherkin.berp -t gherkin-csharp.razor -o $@

.sln_built_debug: Gherkin/Parser.cs $(CS_FILES)
	rm -f $@
	mono --runtime=v4.0 .nuget/NuGet.exe restore Gherkin.CSharp.sln
	xbuild /p:Configuration=Debug
	touch $@

Gherkin/bin/Debug/Gherkin.dll: Gherkin/Parser.cs $(CS_FILES)
	rm -f $@
	xbuild /p:Configuration=Debug
	touch $@

Gherkin/bin/Release/Gherkin.dll: Gherkin/Parser.cs $(CS_FILES)
	rm -f $@
	xbuild /p:Configuration=Release
	touch $@

$(NUNIT):
	mono --runtime=v4.0 .nuget/NuGet.exe install NUnit.Runners -Version 2.6.3 -o packages

dialects.json: ../dialects.json
	cp $< $@
