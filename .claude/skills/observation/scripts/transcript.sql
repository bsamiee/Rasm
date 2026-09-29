-- Messages, models, and tokens of one transcript, variable transcript a Stop transcript_path or a SubagentStop agent_transcript_path
.read .claude/skills/observation/scripts/usage.sql
select * exclude (transcript) from usage(getvariable('transcript'));
