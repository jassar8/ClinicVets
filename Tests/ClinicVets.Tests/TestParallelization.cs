using Xunit;

// Several integration tests mutate the process-global CLINICVETS_DB environment
// variable and shared SQLite files. Running test collections in parallel causes a
// race on that env var (e.g. duplicate seed inserts). Disable parallelization so the
// suite is deterministic; the full suite still runs in about a second.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
