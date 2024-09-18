Feature: Get single leaf

Scenario: Leaf endpoint is not accessible unauthenticated
	When the endpoint /leafs/7a928f69-e5db-4c23-ab3b-feafbe435a1c is called
	Then the status code should be 401

Scenario: Leafs endpoint returns a not found status if the leaf does not exist
	Given a valid user is authenticated
	When the endpoint /leafs/c7969f70-69d4-4c66-bb01-a79039e8e134 is called
	Then the status code should be 404