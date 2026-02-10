# Tailwind CSS Setup

This project uses Tailwind CSS for styling. The CSS is built from source using the Tailwind CLI.

## Files

- `Styles/tailwind.css` - Source file with Tailwind directives
- `tailwind.config.js` - Tailwind configuration
- `package.json` - NPM dependencies for Tailwind CSS
- `wwwroot/css/tailwind.css` - Generated CSS file (committed to git)

## Building

The Tailwind CSS is automatically built as part of the .NET build process via the `BuildTailwindCSS` target in `CRM.V3.Shared.csproj`.

**Build optimization**: 
- In **Debug** mode: CSS is only built if the output file doesn't exist (to speed up development builds)
- In **Release** mode: CSS is always rebuilt (to ensure production has the latest styles)
- NPM install only runs if node_modules doesn't exist

For development, you can manually rebuild CSS when making style changes using the manual build or watch mode commands below.

### Manual Build

To manually build the CSS:

```bash
npm install
npm run build:css
```

### Watch Mode (Development)

To automatically rebuild CSS when source files change:

```bash
npm run watch:css
```

## Configuration

The Tailwind configuration in `tailwind.config.js` includes:
- Custom colors matching the application theme
- Custom border radius values
- Form plugin (@tailwindcss/forms)
- Container queries plugin (@tailwindcss/container-queries)
- Dark mode support

## Important Notes

- **Do not** use the Tailwind CSS CDN in production
- The generated CSS file (`wwwroot/css/tailwind.css`) should be committed to git
- Node modules are ignored via .gitignore
- The CSS is minified for production
