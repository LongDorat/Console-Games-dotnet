# C# Coding Standards

## Naming Conventions

- **PascalCase** for class names, properties, and method names.
- **camelCase** for local variables and method parameters.
- **ALL_CAPS** for constants.

## Indentation

- Use tabs for indentation. Do not use spaces.

## Brace Style

- Use the Allman style for braces:

    ```csharp
    public class Example
    {
	    public void Method()
	    {
		    if (condition)
		    {
			    // code
		    }
	    }
    }
    ```

## Commenting

- Use `//` for single-line comments.
- Use `/* ... */` for multi-line comments.
- Use `//!` for caution or important notices.
- Use `//?` for informational comments.

```csharp
// This is a single-line comment

/*
This is a multi-line comment
spanning multiple lines.
*/

//! This is a caution or important notice

//? This is an informational comment
```

## Language Features

- Prefer `var` when the type is obvious from the right side of the assignment:

    ```csharp
    var list = new List<int>();
    ```

- Use string interpolation over string concatenation:

    ```csharp
    string message = $"Hello, {name}";
    ```

## Error Handling

- Use exceptions for error handling. Do not use return codes.
- Always catch specific exceptions, not general ones:

    ```csharp
    try
    {
	    // code
    }
    catch (SpecificException ex)
    {
	    // handle exception
    }
    ```

## File Organization

- One class per file.
- The file name should match the class name.

## Miscellaneous

- Avoid magic numbers. Use named constants instead.
- Keep methods short and focused. A method should do one thing.

## Example

```csharp
public class Person
{
	private const int MaxAge = 120;
	private string name;
	private int age;

	public string Name
	{
		get { return name; }
		set { name = value; }
	}

	public int Age
	{
		get { return age; }
		set
		{
			if (value < 0 || value > MaxAge)
			{
				throw new ArgumentOutOfRangeException(nameof(value), "Age must be between 0 and 120.");
			}
			age = value;
		}
	}

	public void DisplayInfo()
	{
		Console.WriteLine($"Name: {Name}, Age: {Age}");
	}
}
```
