Feature: Get certificate authorities

Scenario: Returns an empty array if no certificate authorities are registered
	Given the user "test@example.com" is authenticated with the password "t3sTpa55w0rd"
	When the endpoint /cas is called
	Then the status code should be 200
		And the response is an empty array

Scenario: Get certificate authorities returns root authorities in the order created
	Given the following certificate authorities are registered:
		| Id                                   | Name   | Parent |
		| 2194cad4-dc14-4e5c-a217-c51ce9c756be | Root 1 |        |
		| 39cb999d-1ba7-4a9d-9d3c-ada09866cb07 | Root 2 |        |
		| fa3c4626-3efa-4cb5-9251-512c017221f3 | Root 3 |        |
  		And the user "test@example.com" is authenticated with the password "t3sTpa55w0rd"
  	When the endpoint /cas is called
  	Then the response is an array with the fields:
  		| Index | Property | Value                                |
	    | 0     | id       | 2194cad4-dc14-4e5c-a217-c51ce9c756be |
	    | 0     | name     | Root 1                               |
	    | 1     | id       | 39cb999d-1ba7-4a9d-9d3c-ada09866cb07 |
	    | 1     | name     | Root 2                               |
	    | 2     | id       | fa3c4626-3efa-4cb5-9251-512c017221f3 |
	    | 2     | name     | Root 3                               |