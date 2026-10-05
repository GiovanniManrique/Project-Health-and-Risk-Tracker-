"""Embed the current four C# source files in the offline teaching page.

Run from any directory with Python 3. Standard library only.
After changing C# behavior, review lessons.json as well as regenerating the page.
"""
import hashlib
import json
from pathlib import Path

HERE = Path(__file__).resolve().parent
APP = HERE.parent / "ProjectHealthTracker"
FILES = ["Models/StatusTypes.cs", "Models/ProjectItem.cs", "Models/Project.cs", "Program.cs"]


def build():
    sources = {}
    for name in FILES:
        source = (APP / name).read_text(encoding="utf-8-sig")
        sources[name] = {
            "text": source,
            "sha256": hashlib.sha256(source.encode("utf-8")).hexdigest(),
        }
    lessons = json.loads((HERE / "lessons.json").read_text(encoding="utf-8"))
    coverage = {name: set() for name in FILES}
    for lesson in lessons:
        lines = sources[lesson["file"]]["text"].splitlines()
        start, end = lesson["start"], lesson["end"]
        assert 1 <= start <= end <= len(lines), lesson["title"]
        coverage[lesson["file"]].update(range(start, end + 1))
        for note in lesson["notes"]:
            assert start <= note["line"] <= end, (lesson["title"], note)
            assert lines[note["line"] - 1].strip(), (lesson["title"], note)
    for name in FILES:
        for number, line in enumerate(sources[name]["text"].splitlines(), 1):
            assert not line.strip() or number in coverage[name], (name, number)
    payload = json.dumps({"sources": sources, "lessons": lessons}, ensure_ascii=False)
    # JSON lives in a script element, so prevent source text from ending that element.
    payload = payload.replace("<", "\\u003c").replace("&", "\\u0026")
    template = (HERE / "template.html").read_text(encoding="utf-8")
    assert template.count("__GUIDE_DATA__") == 1
    output = template.replace("__GUIDE_DATA__", payload)
    (HERE / "learn.html").write_text(output, encoding="utf-8", newline="\n")
    print(f"Built learn.html: {len(lessons)} lessons, {len(sources)} exact source snapshots.")


if __name__ == "__main__":
    build()
