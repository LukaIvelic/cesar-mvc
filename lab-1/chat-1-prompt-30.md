# Chat 1 - Prompt 30

## User

Make the table have a clear background and borders. I want #fafafa on cells that contain data, others that have th for the column name lets say they get #efefef

---

## Claude

Updated `Views/WeatherForecast/Index.cshtml` with a `<style>` block:
- `th` → `#efefef` background, `1px solid #d0d0d0` border
- `td` → `#fafafa` background, `1px solid #d0d0d0` border
- `border-collapse: collapse` so borders don't double up
