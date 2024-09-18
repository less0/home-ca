Feature: Private key of leafs can be obtained via the respective endpoints

Scenario: Endpoint cannot be called unauthenticated
	Given no user is authenticated
	When the endpoint /leafs/02b4362a-b13a-4183-9cc8-8ebadefe6a01/privateKey is called
	Then the status code should be 401

Scenario: Endpoint returns not found if the leaf does not exist
	Given a valid user is authenticated
	When the endpoint /leafs/4621d26b-5555-42eb-a499-60cb2db11b52/privateKey is called
	Then the status code should be 404

Scenario: Endpoint returns not found if there is no private key for an existing leaf
	Given a valid user is authenticated
		And the following certificate authorities are registered:
			| Id                                   | Name    |
			| 693643ac-c7b7-4a5e-af9a-7f6846deefe9 | Root CA |
		And the following leafs are registered
			| Id                                   | Name                       | Parent                               |
			| 395768ce-cded-4989-bbf3-b9b960108e04 | Leaf without a private key | 693643ac-c7b7-4a5e-af9a-7f6846deefe9 |
	When the endpoint /leafs/395768ce-cded-4989-bbf3-b9b960108e04/privateKey is called
	Then the status code should be 404

Scenario: Endpoint returns the private key if it exists
	Given a valid user is authenticated
		And the following certificate authorities are registered:
			| Id                                   | Name    |
			| 94fa5ca2-2f7d-4aab-b12b-5d9fc10ba92f | Root CA |
		And the following leafs are registered
			| Id                                   | Name                    | Parent                               |
			| 2b31cbba-71c0-41ad-9b78-58bcd3d8c4ad | Leaf with a private key | 94fa5ca2-2f7d-4aab-b12b-5d9fc10ba92f |
		And the leaf "2b31cbba-71c0-41ad-9b78-58bcd3d8c4ad" has a certificate
	When the endpoint /leafs/2b31cbba-71c0-41ad-9b78-58bcd3d8c4ad/privateKey is called
	Then the response is a well-formed PEM private key