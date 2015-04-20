var assert = require('assert');
var Compiler = require('../lib/gherkin/compiler');

describe('Compiler', function () {
  var compile;

  beforeEach(function () {
    var compiler = new Compiler();
    var result = [];
    compiler.on('test-case', function (testCase) {
      result.push({type: 'TestCase', value: testCase});
    });
    compiler.on('test-step', function (testStep) {
      result.push({type: 'TestStep', value: testStep});
    });

    compile = function(path) {
      var feature = require(path);
      compiler.compile(feature);
      return result;
    }
  });

  it("compiles a feature with a single scenario", function () {
    var expected = [
      { type: 'TestCase', value: {
        name: 'Scenario: minimalistic',
        location: { column: 3, line: 3 } } },
      { type: 'TestStep', value: {
        name: 'Given the minimalism',
        location: { column: 5, line: 4 } } }
    ];

    assert.deepEqual(compile('../../testdata/good/minimal.feature.ast.json'), expected);
  });

  it("compiles a feature with a background", function () {
    var expected = [
      { type: 'TestCase',
        value: { name: 'Scenario: minimalistic', location: { column: 3, line: 7 } } },
      { type: 'TestStep',
        value: { name: 'Given the minimalism inside a background', location: { column: 5, line: 4 } } },
      { type: 'TestStep',
        value: { name: 'Given the minimalism', location: { column: 5, line: 8 } } } ];

    assert.deepEqual(compile('../../testdata/good/background.feature.ast.json'), expected);
  });

  it("compiles a scenario outline to test cases", function () {
    var compiled = compile('../../testdata/good/scenario_outline.feature.ast.json');
  });
});
