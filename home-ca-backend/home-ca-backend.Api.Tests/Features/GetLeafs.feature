@nonParallizable
Feature: Get leafs

Scenario: Leafs endpoint can be accessed
	Given the following certificate authorities are registered:
		| Id                                   | Name |
		| e917f41c-43e0-43bd-abf6-18860f6ee03d | Root |
	When the endpoint /cas/e917f41c-43e0-43bd-abf6-18860f6ee03d/leafs is called
	Then the status code should not be 404
		And the status code should not be 405

Scenario: Leafs endpoint can't be accessed unauthenticated
	Given the following certificate authorities are registered:
		| Id                                   | Name |
		| b63c4f39-68ab-4d7e-97e5-18815455f360 | Root |
		And no user is authenticated
	When the endpoint /cas/b63c4f39-68ab-4d7e-97e5-18815455f360/leafs is called
	Then the status code should be 401

Scenario: Endpoint returns an empty array if there are no leafs
	Given a valid user is authenticated
		And the following certificate authorities are registered:
			| Id                                   | Name |
			| fb5d1969-6a7b-4c61-8f87-114f373f2f08 | Root |
  	When the endpoint /cas/fb5d1969-6a7b-4c61-8f87-114f373f2f08/leafs is called
  	Then the response is an empty array	
  	
Scenario: Endpoint returns only matching leafs
	Given a valid user is authenticated
		And the following certificate authorities are registered:
			| Id                                   | Name    |
			| 85f99a6d-56c9-4482-b72a-50ee69fe0d6e | Root I  |
			| 67efbb53-eb15-4eaf-a274-c7e9942b2a32 | Root II |
   		And the following leafs are registered
   			| Id                                   | Name     | Parent                               |
		    | 2320e3cb-9442-4719-92d9-b6e7d4bdb89d | Leaf I   | 85f99a6d-56c9-4482-b72a-50ee69fe0d6e |
		    | 8956606f-d861-4eaf-930b-2cb42ba51df1 | Leaf II  | 85f99a6d-56c9-4482-b72a-50ee69fe0d6e |
		    | 0f45d8e7-30a9-41ef-8f4a-3dd87dced50f | Leaf III | 67efbb53-eb15-4eaf-a274-c7e9942b2a32 |
		    | ea2033f7-489d-4e4f-97b8-b07cae2f9d4f | Leaf IV  | 67efbb53-eb15-4eaf-a274-c7e9942b2a32 |
    When the endpoint /cas/67efbb53-eb15-4eaf-a274-c7e9942b2a32/leafs is called
    Then the response is an array with 2 entry
    	And the response is an array with the fields:
      		| Index | Property | Value                                |
	        | 0     | id       | 0f45d8e7-30a9-41ef-8f4a-3dd87dced50f |
	        | 0     | name     | Leaf III                             |
	        | 1     | id       | ea2033f7-489d-4e4f-97b8-b07cae2f9d4f |
	        | 1     | name     | Leaf IV                              |
         
Scenario: List of leafs does not contain certificate
	Given a valid user is authenticated
		And the following certificate authorities are registered:
			| Id                                   | Name |
			| 3d377e1f-ecff-4ac8-b81d-4c416133a8e4 | Root |
   		And the following leafs are registered
   			| Id                                   | Name | Parent                               |
		    | b877b223-90d0-4b5b-a570-8925f1d4b0f3 | Leaf | 3d377e1f-ecff-4ac8-b81d-4c416133a8e4 |
      	And the leaf "b877b223-90d0-4b5b-a570-8925f1d4b0f3" has a certificate
    When the endpoint /cas/3d377e1f-ecff-4ac8-b81d-4c416133a8e4/leafs is called
    Then the response is an array without the "certificate" field in the items
      