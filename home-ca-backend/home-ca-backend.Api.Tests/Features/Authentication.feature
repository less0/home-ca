Feature: Authentication
    
Scenario: Unauthenticated requests return an Unauthorized (401) status code 
    When the endpoint /cas is called
    Then the status code should be 401