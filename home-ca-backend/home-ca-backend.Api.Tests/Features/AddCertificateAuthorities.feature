Feature: Certificate authorities can be added
   
Scenario: Endpoint is available
    When the endpoint /cas is called with a POST request
    Then the status code should not be 404
        And the status code should not be 405
        
Scenario: Endpoint cannot be called unauthenticated
    When the endpoint /cas is called with a POST request
    Then the status code should be 401
    
Scenario: Endpoint returns a status OK (200) on a valid request
    Given a valid user is authenticated
    When the endpoint /cas is called with a POST request with the data
        | Property | Value                 |
        | Name     | This is a FooBar root |
    Then the status code should be 200