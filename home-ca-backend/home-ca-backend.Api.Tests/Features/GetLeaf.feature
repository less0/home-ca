Feature: Get single leaf

Background: 
	Given the database is empty

Scenario: Leaf endpoint is not accessible unauthenticated
	Given no user is authenticated
	When the endpoint /leafs/7a928f69-e5db-4c23-ab3b-feafbe435a1c is called
	Then the status code should be 401

Scenario: Leafs endpoint returns a not found status if the leaf does not exist
	Given a valid user is authenticated
	When the endpoint /leafs/c7969f70-69d4-4c66-bb01-a79039e8e134 is called
	Then the status code should be 404

Scenario: Status OK is returned if the leaf exists
	Given a valid user is authenticated
		And the following certificate authorities are registered:
			| Id                                   | Name    |
			| 29f8c4e6-2d3e-4279-8b8c-92e2548b0ee4 | Root CA |
		And the following leafs are registered
			| Id                                   | Name | Parent                               |
			| f98fb958-32ca-4486-a95c-5015b1c3fadc | Leaf | 29f8c4e6-2d3e-4279-8b8c-92e2548b0ee4 |
	When the endpoint /leafs/f98fb958-32ca-4486-a95c-5015b1c3fadc is called
	Then the status code should be 200

Scenario: Correct leaf is returned if the leaf exists
	Given a valid user is authenticated
		And the following certificate authorities are registered:
			| Id                                   | Name    |
			| 713fbf30-21b3-4b4c-99a7-ea777ab97fd9 | Root CA |
		And the following leafs are registered
			| Id                                   | Name | Parent                               |
			| 8782d198-5077-49ef-b948-be303cd9e9cf | Leaf | 713fbf30-21b3-4b4c-99a7-ea777ab97fd9 |
	When the endpoint /leafs/8782d198-5077-49ef-b948-be303cd9e9cf is called
	Then the response is an object with the fields
		| Property | Value                                |
		| id       | 8782d198-5077-49ef-b948-be303cd9e9cf |
		| name     | Leaf                                 |

Scenario: Correct leaf is returned if the leaf exists in an intermediate certificate authority
	Given a valid user is authenticated
		And the following certificate authorities are registered:
			| Id                                   | Name            | Parent                               |
			| 713fbf30-21b3-4b4c-99a7-ea777ab97fd9 | Root CA         |                                      |
			| 509cd5c1-09b1-4a8f-842b-7d3d16b43507 | Intermediate CA | 713fbf30-21b3-4b4c-99a7-ea777ab97fd9 |
		And the following leafs are registered
			| Id                                   | Name | Parent                               |
			| 71014c15-86f9-4d74-a8e8-ca29a3a98798 | Leaf | 509cd5c1-09b1-4a8f-842b-7d3d16b43507 |
	When the endpoint /leafs/71014c15-86f9-4d74-a8e8-ca29a3a98798 is called
	Then the response is an object with the fields
		| Property | Value                                |
		| id       | 71014c15-86f9-4d74-a8e8-ca29a3a98798 |
		| name     | Leaf                                 |

Scenario: Leaf is returned with certificate chain
	Given a valid user is authenticated
		And the following certificate authorities are registered:
			| Id                                   | Name    |
			| 32808174-b503-4414-999b-ba814fbb6af9 | Root CA |
		And the following leafs are registered
			| Id                                   | Name      | Parent                               |
			| c0ef6bc0-a501-4d3f-ba6c-b4c459b37437 | Some leaf | 32808174-b503-4414-999b-ba814fbb6af9 |
		And the root certificate authority "32808174-b503-4414-999b-ba814fbb6af9" has a certificate
		And the leaf "c0ef6bc0-a501-4d3f-ba6c-b4c459b37437" has a signed certificate
	When the endpoint /leafs/c0ef6bc0-a501-4d3f-ba6c-b4c459b37437 is called
	Then the property "certificateChain" of the response is not empty