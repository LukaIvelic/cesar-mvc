# Chat 1 - Prompt 21

## User

Okay, but you're telling me for each REST req you will make a file inside of features/weather? I will have 20 files inside of the directory. Isn't it more acceptable to make related files go inside of the dir in features/weather? Isn't that the convention?

---

## Claude

Two fixes:
1. Grouped `Models/` into a subfolder inside `Features/Weather/`
2. Combined each interface with its implementation into one file — they always travel together in feature-folder arch

Final structure:
```
Features/Weather/
├── Models/
│   ├── WeatherForecastViewModel.cs
│   └── CreateWeatherForecastModel.cs
├── WeatherForecast.cs
├── WeatherRepository.cs   ← IWeatherRepository + WeatherRepository
└── WeatherService.cs      ← IWeatherService + WeatherService
```

7 files → 4 files. Updated namespaces in controller, views, and extensions accordingly.
