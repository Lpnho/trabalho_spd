using Grpc.Core;

namespace Freeway.Implementation.GoogleRpc;

internal record GrpcPersistentConnection(
    IServerStreamWriter<Models.Packet> Stream,
    CancellationTokenSource CancellationTokenSource
);