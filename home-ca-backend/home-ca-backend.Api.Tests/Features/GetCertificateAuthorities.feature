Feature: Get certificate authorities
	
Background: 
	Given the database is empty

Scenario: Returns an empty array if no certificate authorities are registered
	Given a valid user is authenticated
	When the endpoint /cas is called
	Then the status code should be 200
		And the response is an empty array

Scenario: Get certificate authorities returns root authorities in the order created
	Given the following certificate authorities are registered:
		| Id                                   | Name   | Parent |
		| 2194cad4-dc14-4e5c-a217-c51ce9c756be | Root 1 |        |
		| 39cb999d-1ba7-4a9d-9d3c-ada09866cb07 | Root 2 |        |
		| fa3c4626-3efa-4cb5-9251-512c017221f3 | Root 3 |        |
  		And a valid user is authenticated
  	When the endpoint /cas is called
  	Then the response is an array with the fields:
  		| Index | Property | Value                                |
	    | 0     | id       | 2194cad4-dc14-4e5c-a217-c51ce9c756be |
	    | 0     | name     | Root 1                               |
	    | 1     | id       | 39cb999d-1ba7-4a9d-9d3c-ada09866cb07 |
	    | 1     | name     | Root 2                               |
	    | 2     | id       | fa3c4626-3efa-4cb5-9251-512c017221f3 |
	    | 2     | name     | Root 3                               |
     
Scenario: Get children only returns the correct intermediate certificate authority
	Given the following certificate authorities are registered:
		| Id                                   | Name           | Parent                               |
		| 14fc416f-0ad2-4402-a34c-6ad50dfe428e | Root 1         |                                      |
		| 39bd84a0-f836-4112-8809-45a7108cb591 | Root 2         |                                      |
		| 1236bbf7-0d4c-4b57-b0ad-f1aacf47a4d6 | Intermediate 1 | 14fc416f-0ad2-4402-a34c-6ad50dfe428e |
		| 75a52497-8729-4bb6-a596-7df8315f917a | Intermediate 2 | 39bd84a0-f836-4112-8809-45a7108cb591 |
		And a valid user is authenticated
	When the endpoint /cas/14fc416f-0ad2-4402-a34c-6ad50dfe428e/children is called
	Then the response is an array with 1 entry
		And the response is an array with the fields:
			| Index | Property | Value                                |
			| 0     | id       | 1236bbf7-0d4c-4b57-b0ad-f1aacf47a4d6 |
			| 0     | name     | Intermediate 1                       |
   
Scenario: Get children returns 404 for non-existing parent ID
	Given a valid user is authenticated
	When the endpoint /cas/14fc416f-0ad2-4402-a34c-6ad50dfe428e/children is called
	Then the status code should be 404