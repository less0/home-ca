Feature: Parameters are validated when adding certificate authorities

Background: 
	Given a valid user is authenticated 

Scenario: Adding a root certificate authority requires a name
	When the endpoint /cas is called with a POST request with the data
		| Property | Value    |
		| Password | p4$$w0rD |
	Then the status code should be 400