# POLE
Portable Object Layout Encoding

POLE is a format for encoding object graphs similar to JSON, but: 

- encoding and decoding are close to zero-alloc, just write bytes into byte[]s
- encoding is close to zero work, i.e. POLE objects have no data fields that need to be serialized; their "fields" are the output buffer.
- decoding is close to zero work, i.e. POLE payload is exatly the memory representation of the POLE object graph
- decoder can read from a sequence of buffers, as opposed to a single contiquous buffer. This enables efficient transimssion without buffer resize-copy.
- decoded object graphs can be all structs, i.e. no heap allocations, but the structs have natural (easy to use) reference semantics
- encoded payloads are typically much smaller than JSON

