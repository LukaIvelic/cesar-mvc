# Chat 1 - Prompt 34

## User

postcss also not recognized as internal or external command on Windows.

---

## Claude

Windows binary resolution issue. Updated package.json scripts to call node directly:

```json
"build:css": "node ./node_modules/postcss-cli/bin/postcss ./wwwroot/css/app.css -o ./wwwroot/css/site.css"
```

Fix:
```bash
npm install -D postcss-cli
npm run build:css
```
