# EestiQuizer

This is a anki card generator for Estonian vocabulary which given a set of words
generates importable ascii Anki files + also collects images for words for which
ekilex has any. Most useful if you have a book
or article or something and you manually collect those words - then this
software collects the rest of the details about that word/words
(even plural since detects homonyms synonyms etc).

If you find this interesting or useful and would like to use it but it is not clear
how then leave an issue and I will include some instructions.
It only supports Windows.

Nowadays as priorities have shifted I don't actively use it but there was a couple months
where it was fulfilling its purpose for simplifying my learnings.

## Quick start

1. Clone the repository and open `EestiQuizer.sln` in Visual Studio (Windows).
2. Build the solution and run `EestiQuizer` from Visual Studio or the output folder.
3. Visit the Settings tab and configure it (like the Ekilex API key)  
   If you don't have and ekilex api key you can get one for free:
   - https://ekilex.ee/login

## How it works

- Input: plain text files containing target words or lists of words.
- Lookup: the app queries Ekilex for word forms, meanings, and example sentences.
- Image collection: downloads images referenced by Ekilex into the configured output folder.
- Output: generates plain-text Anki import files and optional auxiliary data (image files, caches).

## Project structure

- `EestiQuizer/` — main application (WPF UI, application logic).
  - `Ekilex/` — Ekilex integration, request client, processors, and models.
  - `Sonapi/` — legacy Sonapi-related code (kept for reference).  
    https://www.sonapi.ee/#/
  - `Common/` — shared utilities and settings management.
  - `Views/` — WPF XAML views and view models.
- `ekilex/` — Bruno collection templates and examples used for integrations.
- `TestingGrounds/` — can be ignored.

## Future improvements

Although currently not planned, the following applies:

- Finish Todo-s from todo.txt
- Enhance documentation with screenshots and example workflows.
