Feature: Calculator

Scenario: google calculator
    Given url is "https://www.google.com"
    When I enter "2+2" into the search box
    And I click the search button
    Then I should see "4" in the results

