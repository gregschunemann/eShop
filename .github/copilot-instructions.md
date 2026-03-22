# Custom Instructions and Tweaks go here.

## eShop Details

### Product Context
- **Mission & Vision:** [mission](.docs/product/mission.md)
- **Technical Architecture:** [tech-stack](.docs/product/tech-stack.md)
- **Development Roadmap:** [roadmap](.docs/product/roadmap.md)
- **Decision History:** [decisions](.docs/product/decisions.md)

### Development Standards
- **Code Style:** [code-style](.docs/standards/code-style.md)
- **Best Practices:** [best-practices](.docs/standards/best-practices.md)

### Project Management
- **Active Specs:** [specs](.docs/specs/)
- **memory**: Memory MCP tool - Contains graph data for relational information around the project and WIP
- **todo-md**: ToDo MCP Tool - maintains a list of active working items and tasks

## Workflow Instructions

When asked to work on this codebase:

1. **First**, check `todo-md` for any existing ongoing tasks
2. **Then**, pull existing context, notes, and details from the `memory` mcp tool
3. **Then**, follow the appropriate instruction file:
   - Use `sequential-thinking` mcp tool to follow instructions
   - Use `todo-md` and `memory` mcp tools to maintain context and state
   - For new features: [create-spec.instructions.md](.github/instructions/create-spec.instructions.md)
   - For tasks execution: [execute-tasks.instructions.md](.github/instructions/execute-tasks.instructions.md)
4. **Always**, adhere to the standards in the files listed above
5. **Always** use `context7` and `microsoft.docs` to validate usage of SDKs, libraries, and implementation
6. **IMPORTANT** - use `todo-md` and `memory` MCP tools to track and maintain tasks.

## Important Notes

- Product-specific files in `.docs/product/` override any global standards
- User's specific instructions override (or amend) instructions found in `.docs/specs/...`
- Always adhere to established patterns, code style, and best practices documented above
- Always lookup documentation for 3rd party libraries using the `context7` MCP
- Always lookup documentation for Microsoft related technologies, libraries, and SDKs using `microsoft.docs` MCP
- If coding standards do not exist in the `.docs/standards` directory, create the folder and run the `create_standards` task.
