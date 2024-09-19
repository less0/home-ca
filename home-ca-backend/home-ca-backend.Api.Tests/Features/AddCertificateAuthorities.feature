@nonParallelizable
Feature: Certificate authorities can be added
   
Scenario: Endpoint is available
    When the endpoint /cas is called with a POST request
    Then the status code should not be 404
        And the status code should not be 405
        
Scenario: Endpoint cannot be called unauthenticated
    Given no user is authenticated
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
    
Scenario: Root certificate authority with well-formed certificate is created on successful request
    Given a valid user is authenticated
    When the endpoint /cas?password=f00b4r is called with a POST request with the data
        | Property | Value      |
        | Name     | FooRootBar |
    Then there is a certificate authority "FooRootBar" with a well-formed certificate for the returned GUID
    
Scenario: Create intermediate certificate endpoint is available
    When the endpoint /cas/6535025c-500b-4fa3-a3bb-6697d2cc7bb8/children is called with a POST request
    Then the status code should not be 404
        And the status code should not be 405
    
Scenario: Create intermediate certificate endpoint can't be called unauthorized
    Given no user is authenticated
    When the endpoint /cas/a1b1747b-8c3a-4bd6-8a5f-0240f31a6c34/children is called with a POST request
    Then the status code should be 401
    
Scenario: Create intermediate certificate endpoint returns Not Found (404) when parent ID does not exist
    Given a valid user is authenticated
    When the endpoint /cas/121fd8e3-faaf-4324-9da8-5ea8a253b97f/children is called with a POST request with the data
        | Property | Value   |
        | Name     | Qux Baz |
    Then the status code should be 404
    
Scenario: Create intermediate certificate endpoint returns OK (200) on valid request
    Given a valid user is authenticated
        And the following certificate authorities are registered:
            | Id                                   | Name | Parent |
            | 562cbb96-d97c-4a26-b33a-d0039180a6ed | Root |        |
        And the root certificate authority "562cbb96-d97c-4a26-b33a-d0039180a6ed" has a certificate
    When the endpoint /cas/562cbb96-d97c-4a26-b33a-d0039180a6ed/children?password=1234&parentPassword=123456 is called with a POST request with the data
        | Property | Value                              |
        | Name     | Intermediate Certificate Authority |
    Then the status code should be 200
       
Scenario: Create intermediate certificate endpoint returns GUID on valid request
    Given a valid user is authenticated
        And the following certificate authorities are registered:
            | Id                                   | Name             |
            | 9f87d75e-5336-4c81-8479-f5a43bdb7be1 | Rooty McRootface |
        And the root certificate authority "9f87d75e-5336-4c81-8479-f5a43bdb7be1" has a certificate
    When the endpoint /cas/9f87d75e-5336-4c81-8479-f5a43bdb7be1/children?password=1234&parentPassword=123456 is called with a POST request with the data
        | Property | Value               |
        | Name     | Intermediate thingy |
    Then the response is a valid GUID

Scenario: Intermediate certificate authority is created with certificate
    Given a valid user is authenticated
        And the following certificate authorities are registered:
            | Id                                   | Name    |
            | 9dc6996b-d4ee-4816-ad4a-7a238e108b24 | Root CA |
        And the root certificate authority "9dc6996b-d4ee-4816-ad4a-7a238e108b24" has a certificate
    When the endpoint /cas/9dc6996b-d4ee-4816-ad4a-7a238e108b24/children?password=password&parentPassword=123456 is called with a POST request with the data
        | Property | Value           |
        | Name     | Intermediate CA |
    Then there is a certificate authority "Intermediate CA" with a well-formed certificate for the returned GUID

Scenario: Creating an intermediate certificate fails with forbidden (403) if the parent password mismatches
    Given a valid user is authenticated
        And the following certificate authorities are registered:
            | Id                                   | Name |
            | 8229eece-dfdf-4d01-8f79-5c30b9b1e7ba | root |
        And the root certificate authority "8229eece-dfdf-4d01-8f79-5c30b9b1e7ba" has a certificate
    When the endpoint /cas/8229eece-dfdf-4d01-8f79-5c30b9b1e7ba/children?password=abcdef&parentPassword=654321 is called with a POST request with the data
        | Property | Value           |
        | Name     | Intermediate CA |
    Then the status code should be 403