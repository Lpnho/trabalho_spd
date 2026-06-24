using Grpc.Core;

namespace Freeway.Implementation.GoogleRpc.Utils;

internal record GrpcPersistentConnection(
    IServerStreamWriter<Models.Packet> Stream,
    CancellationTokenSource CancellationTokenSource
);