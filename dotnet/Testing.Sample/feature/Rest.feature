Feature: Rest

Scenario: Dog api
    Given I have a dog api
    When I send a GET request to "/api/breeds/list/all"
    Then the response code should be 200
    And the response should be in JSON
    And the response should contain "affenpinscher"

