Feature: Calculator

  Background: 
    Given I have a calculator

  Scenario Outline: Add two numbers
    Given I have numbers <a> and <b> as input
    And I add more numbers
      | number |
      |      1 |
      |      2 |
    Then I should get an output of <c>

    Examples: 
      | a | b | c |
      | 1 | 2 | 6 |
      | 2 | 3 | 8 |
