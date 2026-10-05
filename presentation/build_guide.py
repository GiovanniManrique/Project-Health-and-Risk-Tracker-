"""Embed the current three C# source files in the offline teaching page.

Run from any directory with Python 3. Standard library only.
After changing C# behavior, review lessons.json as well as regenerating the page.
"""
import hashlib
import json
import re
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
    guide = json.loads((HERE / "lessons.json").read_text(encoding="utf-8"))
    lessons = guide['lessons']
    for lesson in lessons:
        assert lesson["route"] in ("run", "ai")
        for block in lesson["blocks"]:
            lines = sources[block["file"]]["text"].splitlines()
            assert 1 <= block["start"] <= block["end"] <= len(lines), lesson["title"]
            for key in ['simple', 'technical', 'why', 'before', 'after', 'say']:
                assert block[key].strip(), (lesson['title'], key)
            for row in block['rows']:
                assert block['start'] <= row[0] <= block['end'], row
    assert len([lesson for lesson in lessons if lesson["route"] == "run"]) == 10
    assert len([lesson for lesson in lessons if lesson["route"] == "ai"]) == 10
    for ref in guide['references']:
        assert 1 <= ref['start'] <= ref['end'] <= len(sources[ref['file']]['text'].splitlines())
    for step in guide['study']:
        for block in step['blocks']:
            assert 1 <= block['start'] <= block['end'] <= len(sources[block['file']]['text'].splitlines())
    assert sum(step['minutes'] for step in guide['study']) == guide['studyMinutes']
    assert 10 <= guide['studyMinutes'] <= 15
    assert set(item['name'] for item in guide['coverage']) == set(name for step in guide['study'] for name in step['methods'])
    # Build a readable request example from the current source's sample literals.
    # The model instance ID and response below are labelled instructional examples.
    program = sources['Program.cs']['text']
    inventory = program.split('Project inventory = new Project(', 1)[1].split('Project training =', 1)[0]
    def literal(field, text=inventory):
        match = re.search(r'\b' + re.escape(field) + r'\s*=\s*("(?:\\.|[^"\\])*")', text)
        assert match, field
        return json.loads(match[1])
    likelihood = int(re.search(r'RiskLikelihood\s*=\s*(\d+)', inventory)[1])
    impact = int(re.search(r'RiskImpact\s*=\s*(\d+)', inventory)[1])
    name = json.loads(re.search(r'2,\s*("[^"]+")', inventory)[1])
    facts = dict(Name=name, RiskTitle=literal('RiskTitle'), RiskOwner=literal('RiskOwner'),
                 RiskStatus='Open', RiskLikelihood=likelihood, RiskImpact=impact,
                 RiskScenario=literal('RiskScenario'), RiskPlan=literal('RiskPlan'))
    assert 'inventory.ChangeStatus(3, ItemStatus.Open)' in inventory
    assert (likelihood, impact) == (4, 5), 'Re-review the worked health example after sample changes.'
    health = 'OffTrack'
    prompt = (f'Fictional project: {name}\nC# project health: {health}\n'
              f'Risk: {facts["RiskTitle"]} | Owner: {facts["RiskOwner"]} | Status: Open\n'
              f'Likelihood: {likelihood}/5 | Impact: {impact}/5 | Priority score: {likelihood*impact}/25 (not a percentage)\n'
              f'Scenario: {facts["RiskScenario"]}\nCurrent plan: {facts["RiskPlan"]}')
    system_segment = re.search(r'string instructions\s*=\s*(.*?);\s*\n', program, re.S)[1]
    instruction = ''.join(json.loads(s) for s in re.findall(r'"(?:\\.|[^"\\])*"', system_segment))
    assert instruction.endswith('Return only the short explanation.')
    model_key = literal('ModelKey', program)
    url = literal('BionicUrl', program)
    request = dict(model='qwen-example-instance', input=prompt, system_prompt=instruction,
                   reasoning='off', stream=False, store=False, max_output_tokens=500, integrations=[])
    model_list = dict(models=[dict(key=model_key, loaded_instances=[dict(id='qwen-example-instance')])])
    response = dict(output=[dict(type='reasoning', content='Illustrative ignored entry.'),
                            dict(type='message', content='Risk: Scanner delay. Why: Delivery is unconfirmed. Next action: Use manual item numbers for the demo.')])
    printed = 'LOCAL AI ANSWER - ' + model_key + '\n' + response['output'][1]['content']
    guide['wire'] = dict(facts=facts, health=health, prompt=prompt, request=request,
                         modelList=model_list, response=response, printed=printed, url=url)
    payload = json.dumps({"sources": sources, "guide": guide}, ensure_ascii=False)
    # JSON lives in a script element, so prevent source text from ending that element.
    payload = payload.replace("<", "\\u003c").replace("&", "\\u0026")
    template = (HERE / "template.html").read_text(encoding="utf-8")
    assert template.count("__GUIDE_DATA__") == 1
    output = template.replace("__GUIDE_DATA__", payload)
    (HERE / "learn.html").write_text(output, encoding="utf-8", newline="\n")
    print(f"Built learn.html: {len(lessons)} lessons, {len(sources)} exact source snapshots.")


if __name__ == "__main__":
    build()
