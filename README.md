<h1 align="center">LexiCore</h1>
<h3 align="center">High-performance, zero-allocation DSL processing engine for .NET</h3>

<p align="center">
  <img src="https://img.shields.io/badge/version-0.1.0-blue" />
  <img src="https://img.shields.io/badge/status-pre--alpha-orange" />
</p>


## Installation
LexiCore is currently distributed as source-only (no NuGet package yet).
API and structure are subject to change.

You can use it by cloning the repository:
```bash
git clone https://github.com/ice_void/lexicore.git
```


## What is LexiCore
LexiCore is a high-performance, zero-allocation DSL processing engine for .NET
that parses structured text into a token-based IR for flexible rendering and transformation.

---

### Why it exists
LexiCore was created to provide a infrastructure for working with human-readable, DSL-like text systems.

In many real-world systems, text formatting is fragmented across multiple incompatible approaches:
- template placeholders (e.g. `{0}`, `{name}`)
- markup languages (e.g. BBCode, HTML-like tags)
- ad-hoc styling systems embedded in strings

This fragmentation results in complex syntax and lack of uniform rules,
often forcing the creation of ad-hoc internal tools that require ongoing maintenance,
which often leads to errors, especially in UI systems, localization pipelines, and game development workflows.

LexiCore aims to unify these approaches into a composable DSL system with customizable rules,
while remaining readable for non-programmers and extensible for domain-specific use cases.

---

### Examples
**Quick example (DSL => rendered output):**

`[<Warn>]Attention: {Message}` => <span style="color: orange"><b>Attention: Fox was not found</b></span>

> **Note:** All examples use a conceptual `LexiCoreEngine` API, which is subject to change in future versions.

#### Template & partial style closing
Closes only `bold`, while `italic` remains active.
```cs
string text = "[<text-style=italic,bold>]Meow, {User}[/text-style=bold]! [<color=green>]Welcome back!";

var engine = new LexiCoreEngine();

var bindings = new Dictionary<string, string>
{
    ["User"] = "Lyrica"
};
var ansi = engine.Render(text, AnsiRenderer, bindings);
var html = engine.Render(text, HtmlRenderer, bindings);
```
**Rendered:**

<i><b>Meow, Lyrica</b>! <span style="color: green;">Welcome back!</span></i>

<details><summary>Raw output</summary>

**Output (ANSI):**
```
\x1b[3;1mMeow, Lyrica\x1b[22m! \x1b[32mWelcome back!\x1b[0m
```

**Output (HTML):**
```
<i><b>Meow, Lyrica</b>! <span style="color: green;">Welcome back!</span></i>
```
</details>

#### Presets
Presets act as reusable style macros.
```cs
string text = "[<Error>]Fatal error[</>]";

var engine = new LexiCoreEngine(presets: new Dictionary<string, string>
{
	["Error"] = "[<color=red text-style=bold>]"
});

engine.Render(text, AnsiRenderer);
engine.Render(text, HtmlRenderer);
```
**Rendered:**

<b><span style="color: red;">Fatal error</span></b>

<details><summary>Raw output</summary>

**Output (ANSI):**
```
\x1b[1;31mFatal error\x1b[0m
```

**Output (HTML):**
```
<b><span style="color: red;">Fatal error</span></b>
```
</details>

#### Presets override & isolation
Local styles override presets without affecting their outer scope.
```cs
string text = "[<Label>]Welcome, [<text-style=bold>]Ao-BBS «[<color=cyan>]Lyrica[</color>]»[</>]!";

var engine = new LexiCoreEngine(presets: new Dictionary<string, string>
{
	["Label"] = "[<color=lightgreen>]"
});

engine.Render(text, AnsiRenderer);
engine.Render(text, HtmlRenderer);
```
**Rendered:**

<span style="color: lightgreen;">Welcome, <b>Ao-BBS «<span style="color: cyan;">Lyrica</span>»</b></span>!

<details><summary>Raw output</summary>

**Output (ANSI):**
```
\x1b[92mWelcome, \x1b[1mAo-BBS «\x1b[96mLyrica\x1b[92m»\x1b[22m!\x1b[0m
```

**Output (HTML):**
```
<span style="color: lightgreen;">Welcome, <b>Ao-BBS «<span style="color: cyan;">Lyrica</span>»</b></span>!
```
</details>


## Features (Implemented)

### Core
- Lexer pipeline with dispatcher-based routing
    - Extensible sub-lexer infrastructure
- Zero-allocation context for runtime configuration (diagnostics, buffering)
- Sub-lexers:
    - Configurable template lexer
- Diagnostic system (public contract):
    - Diagnostic IDs (unstable)
    - Diagnostic codes (stable, external-facing)
- Centralized syntax rules
- Structured token model (templates, styles)

### Auxiliary
- Low-level parsing utilities for structured text processing


## Overview
LexiCore follows a text processing pipeline:
- **Lexing** — transforms raw input into a stream of tokens
- **Parsing & IR building** — builds an intermediate representation from tokens
- **Caching** — compiled structures can be reused to avoid repeated processing
- **Rendering** — transforms IR into a target format (e.g. ANSI, HTML)

### Design Principles

- **Presets** — reusable style macros that expand into structured tokens
- **Composable styles** — styles are layered and follow stack semantics
- **Implicit closing** — styles are implicitly closed at the end of input
- **Partial closing** — individual properties, values, or presets can be closed independently

## Roadmap

### Phase 1 — Foundation
- Complete test coverage for existing functionality
- Finalize lexer behavior and template processing
- Stabilize diagnostics and error handling

### Phase 2 — Styles & Parsing
- Implement full style system
- Introduce parsing with partial compilation into a mixed IR/syntax stream

### Phase 3 — Engine & Caching
- Implement rendering engine (core processing layer)
- Add caching for compiled structures
- Introduce centralized configuration system

### Phase 4 — Rendering
- Implement standard renderers (ANSI, HTML, BBCode, Markdown, etc.)
- Optimize rendering pipeline
- Explore partial caching within renderers

### Phase 5 — Public API
- Design and stabilize external API
- Enable integration with third-party applications

### Future considerations
- Source generators for compile-time optimizations
- Advanced performance improvements (e.g. pointer-based processing)


Author Note
---
Initially, LexiCore didn't have any goals like its current ones.
Its core was the `TextParsingExtensions` utility for finding pairwise balanced delimiters
and a primitive finite state machine, without any additional features.

At the time, it was simply a small tool, consisting of a few hundred lines of code, for internal game development.
However, at some point, the decision was made to expand it into something more
when it became clear that no existing solution could fully solve the problem on its own,
and that combining them introduced additional overhead to make them work together.

I sincerely hope that this project will be useful to someone else.
