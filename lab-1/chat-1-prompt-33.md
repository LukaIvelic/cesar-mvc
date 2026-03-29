# Chat 1 - Prompt 33

## User

npm install succeeded but npm run build:css failed:
'tailwindcss' is not recognized as an internal or external command

Okay that works, what's next?

---

## Claude

Build actually failed. In Tailwind v4, the build goes through PostCSS CLI not the tailwind CLI.

Fix:
```bash
npm install -D postcss-cli
npm run build:css
```

Updated package.json scripts to use `postcss` instead of `tailwindcss`.

Next: link `wwwroot/css/site.css` in `Views/Shared/_Layout.cshtml` inside `<head>`.
