# Surface Pro 4 Resource Optimization Skill

## Purpose
Optimize build, compilation, linting, and terminal execution commands for resource-constrained environments (specifically Surface Pro 4) to mitigate thermal throttling and memory pressure.

## Operational Instructions
1. **Prevent Thermal Throttling & Memory Spikes**:
   - Avoid launching multiple heavy background processes concurrently.
   - Limit process parallelism to avoid CPU overheating and RAM saturation.
2. **Sequential Execution Preference**:
   - Prefer sequential task execution for compilation, building, and linting tasks over concurrent/parallel runs.
3. **Disk I/O Auditing**:
   - Flag any scripts, file watchers, or dependencies that cause excessive disk I/O or continuous disk polling.
