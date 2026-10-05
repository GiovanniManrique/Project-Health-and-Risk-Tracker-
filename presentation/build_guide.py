"""Embed the current three C# source files in the offline teaching page.

Run from any directory with Python 3. Standard library only.
After changing C# behavior, review lessons.json as well as regenerating the page.
"""
import hashlib
import json
from pathlib import Path

HERE = Path(__file__).resolve().parent
APP = HERE.parent / "ProjectHealthTracker"
FILES = ["Program.cs", "Models/Project.cs", "Models/StatusTypes.cs"]


def build():
    sources = {}
    for name in FILES:
        source = (APP / name).read_text(encoding="utf-8-sig")
        sources[name] = {
            "text": source,
            "sha256": hashlib.sha256(source.encode("utf-8")).hexdigest(),
        }
    lessons = json.loads((HERE / "lessons.json").read_text(encoding="utf-8"))
    for lesson in lessons:
        assert lesson["route"] in ("run", "ai")
        for block in lesson["blocks"]:
            lines = sources[block["file"]]["text"].splitlines()
            assert 1 <= block["start"] <= block["end"] <= len(lines), lesson["title"]
    assert len([lesson for lesson in lessons if lesson["route"] == "run"]) == 10
    assert len([lesson for lesson in lessons if lesson["route"] == "ai"]) == 10
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
