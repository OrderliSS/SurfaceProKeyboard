# Logic Loops & Mechanics Audit Skill

## Purpose
Audit logic loops, sequence escalation mechanics, and scheduled virtual economy events.

## Audit Guidelines
1. **12-Day Combat Escalation Verification**:
   - Verify logic loops controlling combat difficulty scaling and enemy wave strength over a 12-day escalation sequence.
   - Test boundary transitions (Day 1 through Day 12) to ensure state counters increment properly and cap or reset as designed.
2. **Virtual Economy Debt Collection Rules**:
   - Ensure virtual economy debt collectors strictly demand payment on **Days 6–7** only.
   - Audit conditional triggers to guarantee debt collection notices, payment prompts, or penalties do not fire prior to Day 6 or past Day 7 unless designed for overflow handling.
3. **Automated Logic Audits**:
   - Run logic unit test suites covering daily event schedules and state transition handlers.
