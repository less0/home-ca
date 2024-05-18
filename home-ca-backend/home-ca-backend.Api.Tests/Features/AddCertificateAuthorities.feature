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
    
Scenario: Endpoint returns the GUID of the created certificate authority
    Given a valid user is authenticated
    When the endpoint /cas?password=123456 is called with a POST request with the data
        | Property | Value |
        | Name     | r007  |
    Then the response is a valid GUID
    
Scenario: Root certificate authority is created on successful request
    Given a valid user is authenticated
    When the endpoint /cas?password=f00b4r is called with a POST request with the data
        | Property | Value      |
        | Name     | FooRootBar |
    Then there is a root certificate authority "FooRootBar" with the returned GUID