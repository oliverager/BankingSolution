Feature: BSRun batch processing
  The BSRun service processes scheduled collections in two phases:
  - Notify upcoming collections
  - Collect due collections

  Background:
    Given the system is running

  Scenario: Notify upcoming collections marks them as Notified
    Given an active mandate exists for BS
    And a collection is created with due date 7 days from today and amount 199
    When I run BS notify upcoming with daysAhead 7
    Then the notify result should report 1 notified

  Scenario: Collect due approved collections transfers money and marks as Collected
    Given an active mandate exists for BS
    And a collection is approved with due date today and amount 250
    When I run BS collect due
    Then the collect result should report 1 collected

  Scenario: Collect due fails when mandate is cancelled
    Given a cancelled mandate exists for BS
    And a collection is approved with due date today and amount 250
    When I run BS collect due
    Then the collect result should report 0 collected
    And the last collection status should be Failed with reason "MandateNotActive"

    
