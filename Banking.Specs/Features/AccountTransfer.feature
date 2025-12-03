Feature: Account money transfer
  In order to move money between my accounts
  As a banking customer
  I want to transfer funds while respecting limits and balances

  Scenario: Successful transfer between active accounts
    Given a Standard customer with an account "A1" having balance 1000
    And an active account "A2" for the same customer with balance 100
    When the customer transfers 200 from "A1" to "A2"
    Then the transfer should succeed
    And the balance of "A1" should be 800
    And the balance of "A2" should be 300

  Scenario: Transfer fails due to insufficient balance
    Given a Standard customer with an account "A1" having balance 100
    And an active account "A2" for the same customer with balance 0
    When the customer transfers 500 from "A1" to "A2"
    Then the transfer should be rejected with reason "InsufficientBalance"
