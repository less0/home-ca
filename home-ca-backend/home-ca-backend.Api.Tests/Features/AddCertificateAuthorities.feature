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
        | Password | 5tR0ngp4$$w0rd        |
    Then the status code should be 200
    
Scenario: Endpoint returns the GUID of the created certificate authority
    Given a valid user is authenticated
    When the endpoint /cas is called with a POST request with the data
        | Property | Value        |
        | Name     | r007         |
        | Password | r007b$r[ABCD |
    Then the response is a valid GUID
    
Scenario: Root certificate authority with well-formed certificate is created on successful request
    Given a valid user is authenticated
    When the endpoint /cas is called with a POST request with the data
        | Property | Value        |
        | Name     | FooRootBar   |
        | Password | f00b4rQuXb4% |
    Then there is a certificate authority "FooRootBar" with a well-formed certificate for the returned GUID

Scenario: Root certificate is created with a lifetime of 10 years
    Given a valid user is authenticated
    When the endpoint /cas?password=quxbaz is called with a POST request with the data
        | Property | Value        |
        | Name     | QuxRootBaz   |
        | Password | jht[Eo82N37u |
    Then there is a certificate with a lifetime of 10 years for the returned GUID
    
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
    When the endpoint /cas/562cbb96-d97c-4a26-b33a-d0039180a6ed/children is called with a POST request with the data
        | Property       | Value                              |
        | Name           | Intermediate Certificate Authority |
        | Password       | p455w0rd                           |
        | ParentPassword | 123456                             |
    Then the status code should be 200
       
Scenario: Create intermediate certificate endpoint returns GUID on valid request
    Given a valid user is authenticated
        And the following certificate authorities are registered:
            | Id                                   | Name             |
            | 9f87d75e-5336-4c81-8479-f5a43bdb7be1 | Rooty McRootface |
        And the root certificate authority "9f87d75e-5336-4c81-8479-f5a43bdb7be1" has a certificate
    When the endpoint /cas/9f87d75e-5336-4c81-8479-f5a43bdb7be1/children is called with a POST request with the data
        | Property       | Value               |
        | Name           | Intermediate thingy |
        | Password       | 1234                |
        | ParentPassword | 123456              |
    Then the response is a valid GUID
       
Scenario: Intermediate certificate authorities are created with a certificate
    Given a valid user is authenticated
        And the following certificate authorities are registered:
            | Id                                   | Name             |
            | 3120c450-972a-4d97-b2c6-7e4e943115ba | Rooty McRootface |
        And the root certificate authority "3120c450-972a-4d97-b2c6-7e4e943115ba" has a certificate
    When the endpoint /cas/3120c450-972a-4d97-b2c6-7e4e943115ba/children is called with a POST request with the data
        | Property       | Value                              |
        | Name           | Intermediate Certificate Authority |
        | Password       | 1234                               |
        | ParentPassword | 123456                             |
    Then there is a certificate with a lifetime of 3 years for the returned GUID

Scenario: Intermediate certificate authority is created with certificate
    Given a valid user is authenticated
        And the following certificate authorities are registered:
            | Id                                   | Name    |
            | 9dc6996b-d4ee-4816-ad4a-7a238e108b24 | Root CA |
        And the root certificate authority "9dc6996b-d4ee-4816-ad4a-7a238e108b24" has a certificate
    When the endpoint /cas/9dc6996b-d4ee-4816-ad4a-7a238e108b24/children is called with a POST request with the data
        | Property       | Value           |
        | Name           | Intermediate CA |
        | Password       | passw0rd        |
        | ParentPassword | 123456          |
    Then there is a certificate authority "Intermediate CA" with a well-formed certificate for the returned GUID

Scenario: Creating an intermediate certificate fails with forbidden (403) if the parent password mismatches
    Given a valid user is authenticated
        And the following certificate authorities are registered:
            | Id                                   | Name |
            | 8229eece-dfdf-4d01-8f79-5c30b9b1e7ba | root |
        And the root certificate authority "8229eece-dfdf-4d01-8f79-5c30b9b1e7ba" has a certificate
    When the endpoint /cas/8229eece-dfdf-4d01-8f79-5c30b9b1e7ba/children is called with a POST request with the data
        | Property       | Value           |
        | Name           | Intermediate CA |
        | Password       | abcdef          |
        | ParentPassword | 654321          |
    Then the status code should be 403