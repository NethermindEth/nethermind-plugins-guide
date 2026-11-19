# 05 Invalid chain dump

A complete, production-style plugin: when the node rejects an invalid block, it zips the diagnostics the node wrote for the block and uploads the archive to S3-compatible storage (AWS S3, MinIO, Cloudflare R2...), for triage on test networks and devnets.

It shows what the earlier chapters do not:

- **A third-party dependency** (`AWSSDK.S3`), shipped next to the plugin in the plugins folder.
- **Fail-fast configuration**: an incomplete S3 configuration throws `InvalidConfigurationException` when the node starts, instead of silently uploading nothing.
- **Changing another config**: the module extends `Init.AutoDump` so the node writes every kind of diagnostic file.
- **An extension point of the node**: `IInvalidBlockDumpHandler` hands every registered handler the files of each invalid-block dump.
- **Background work with a bounded queue**: uploads run off the node's threads, a failed or timed-out upload does not stop the next ones, and shutdown waits a bounded time for queued uploads, so the dump of a block the node exits on is still uploaded.
- **Testing through the node**: the tests reject a block in a test node and check the uploaded archive, including when the node shuts down right after the rejection.

Read [InvalidChainDumpPlugin.cs](./Nethermind.InvalidChainDump/InvalidChainDumpPlugin.cs), [InvalidChainDumpUploader.cs](./Nethermind.InvalidChainDump/InvalidChainDumpUploader.cs), [S3DumpArchiveStore.cs](./Nethermind.InvalidChainDump/S3DumpArchiveStore.cs), then [the tests](./Nethermind.InvalidChainDump.Test/InvalidChainDumpPluginTests.cs).

> `IInvalidBlockDumpHandler` is added to Nethermind by [NethermindEth/nethermind#11030](https://github.com/NethermindEth/nethermind/pull/11030). Until that is in a release, this chapter builds only against a Nethermind version that has it, like the one in the submodule.

## Configuration

| Option | Default | |
|---|---|---|
| `InvalidChainDump.ServiceUrl` | | The S3 endpoint, for example `http://127.0.0.1:9000`. Setting it enables the plugin. |
| `InvalidChainDump.BucketName` | `invalid-blocks` | |
| `InvalidChainDump.AccessKey`, `SecretKey` | | Required. |
| `InvalidChainDump.Region` | `us-east-1` | Set it for AWS buckets in another region. |
| `InvalidChainDump.ObjectKeyPrefix` | `dumps` | |
| `InvalidChainDump.UploadTimeoutMs` | `300000` | |

Each archive is `<prefix>/dump-<block hash>_<UTC time>.zip` and holds the block RLP, the receipts, Parity- and Geth-style traces of its re-execution, and a `manifest.txt` with the block's hash, number, parent, transaction count, timestamp and the client version. A failed upload leaves the archive in the temporary folder and logs its path.

On shutdown the node first lets a running dump finish (up to 35 seconds), then the plugin waits for queued uploads (up to 35 more). Docker stops a container after 10 seconds by default, so give the container more time, `docker run --stop-timeout 90 ...`, if the dump of the block a node exits on (`Init.ExitOnInvalidBlock`) must be uploaded.

## Try it with MinIO

The plugin uploads only when the node rejects a block, which a healthy network rarely gives you: the [tests](./Nethermind.InvalidChainDump.Test/InvalidChainDumpPluginTests.cs) reject a block on purpose, and devnets and shadow forks are where it pays off.

```sh
docker run -d -p 9000:9000 -e MINIO_ROOT_USER=minio -e MINIO_ROOT_PASSWORD=minio123 minio/minio server /data
# Create the invalid-blocks bucket in the MinIO console or with `mc mb`, then:
dotnet build Nethermind.InvalidChainDump -c release
dotnet run --project ../nethermind/src/Nethermind/Nethermind.Runner -c release -- \
  --config hoodi --plugins-dir "$PWD/../artifacts/plugins" \
  --InvalidChainDump.ServiceUrl http://127.0.0.1:9000 \
  --InvalidChainDump.AccessKey minio --InvalidChainDump.SecretKey minio123
```
