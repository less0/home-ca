Feature: Authentication
    
Scenario: Unauthenticated requests return an Unauthorized (401) status code 
    When the endpoint /cas is called
    Then the status code should be 401
    
Scenario: Authenticated request returns an Ok (200) status code
    Given the user "test@example.com" is authenticated with the password "t3sTpa55w0rd"
    When the endpoint /cas is called
    Then the status code should be 200