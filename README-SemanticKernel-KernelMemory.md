I not use qdrant from semantic kernel, it doesn't work correctly. I can't inject it, documentation on day 30.12.2024 is incorrect or maybe I can't uderstand it correctly.

### Memory
I don't useSemantic Kernel Memory I use Kernel Memory (IKernelMemory) 
KernelMemory connect to qdrant to read data.
KernelMemory store Volatile data.
KernelMemory is generally using by SK plugin.
KernelMemory read from Qdrant

### Qdrant
For Non-Volatile data I use Qdrant
I use Qdrant by .NET SDK.
For chunking data TextChunker from Semantic Kernel for now is experimental.
When I add files, they are first uploaded to Azure Blob Storage and then I use Qdrant to index them.

### File Storage
For Non-Volatile data I use Azure Blov Storage

### Chat History
