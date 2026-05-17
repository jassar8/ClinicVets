using Xunit;

// AppData + DbPaths tests mutate process-wide static paths; run this assembly sequentially.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
