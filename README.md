# NES# Core

## Description

The purpose of this library is to allow you to use C# to write reusable libraries for generating NES ROMs. Games can be written in C# directly in Visual Studio,
and adds all the existing benefits such as strong typing, intellisense, easy refactoring, and modularization. There is no need to have a multi-phase build
process--any scripts needed to generate tables, import data, generate logs or debug files, etc. can all be done within the same project. The result is an
executable that outputs a ROM file ready to play in an NES emulator or flash to a cartridge. The benefit of an executable is that it can also receive input in
the form of parameters, JSON config files, and CHR files, so it can be passed along to other non-programmer team members to work on levels, graphics, or sound,
and test builds without requiring developer tools.

The library contains classes and functions that wrap common functionality that allows you to write code in a higher level format without compromising the
generated code. To achieve this, it does not try to shoehorn another language into 6502 assembly, but instead has several options that appear similar to
concepts of higher level languages, to provide flexibility in writing performant code.

## Features

### Data Types
* VByte - single byte and all chainable methods for operating on values
* VDecimal - variants of values representing integer and fractional parts
* VarN - sequential operations for longer values, from low to high bytes, using generator functions
* Pointers - allow for indexed indirect and indirect indexed addressing
* Structs - define and dimension structs in one instruction, and easily use them in arrays
* Arrays - define and easily use ranges of values with standard C# indexing syntax
* Struct of Arrays and Array of Structs

### ROM output
* Presently supports mapper 0 and 30, with a flexible way to add additional mappers and idiosyncrasies
* Allows for binary output without an iNES header, for testing generated assembly
* Support for unsized banks, to support mappers with no fixed bank, which will require code and reset vector duplication across all banks
* Attributes for methods which specify whether code is Data, immediate code, or a subroutine
* Detailed output including bank and RAM usage
* Assembly output to validate generated code

### Emulator integration
* Debug symbols for Mesen (TODO: FCEUX)

### RAM
* Easily dimension variables from chunks of RAM (ZP, remaining non-stack RAM), and define chunks when needed to ensure page boundaries aren't crossed
* Write banks using persistent variables first. Then use RAM.Remainder() to retrieve a new RAM instance
that can be cleared and reused across all scenes.

### Loops
* Common looping patterns such as ascending or descending to 0 on a given index register have shorthand methods
* Conditional loops have several more formats including pre- and post-condition
* Infinite loops
* ForEach for operating on Struct-of-Arrays
* Shorthand for iterating ranges

### Conditions
* Automatic conversion of branching and jumping depending on block size
* Any(condition, condition...) and All(condition, condition...) for "&&" and "||" equivalent multiple conditions, which use short circuit evaluation
* If(condition, block) -- if
* If(Option(condition, block), Option(condition, block), Default(block)) -- if/else if/else

### General
* Chainable helpers on all registers, address, and variable types to concisely express intent
* Stack methods to show preservation/restoration of stashed register values
* Streams for writing/reading from memory-mapped locations
* Constants and helper methods for working with the NES architecture
* Compile errors when modifying indexing registers when used as a loop index, and a means of safely identifying areas where it's necessary
* Simple assembly parsing for including files for a sound engine
