# Native AOT and Trimming Guide

## 1. Zero-Reflection Guarantee

`EricksonLopez.Events` has been built from day zero for Native AOT and Trimming:
- No `Assembly.GetTypes()` or dynamic reflection scans.
- No `MakeGenericType` or runtime code generation (`IL.Emit` / `Expression.Compile`).
- Static compile-time registration with `EventTypeDescriptor` and `FrozenDictionary`.
- Roslyn Incremental Generator (`EricksonLopez.Events.Generators`) precomputes metadata and type resolvers at compile time.

## 2. Publishing Native AOT Application

To publish a self-contained Native AOT executable:

```bash
dotnet publish -c Release -r win-x64 -p:PublishAot=true
```

Or for Linux:

```bash
dotnet publish -c Release -r linux-x64 -p:PublishAot=true
```

## 3. Trimming Verification

To verify trimming without full AOT compilation:

```bash
dotnet publish -c Release -p:PublishTrimmed=true
```

The build will complete with **zero trim warnings** and minimal binary footprint.
