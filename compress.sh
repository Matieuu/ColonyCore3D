find . -type f \
\( -name "*.rs" -o -name "*.cs" -o -name "*.vert" -o -name "*.frag" -o -name "*.toml" -o -name "*.csproj" -o -name "*.md" \) \
-not -path "*/target/*" \
-not -path "*/bin/*" \
-not -path "*/obj/*" \
-not -path "*/.git/*" \
-exec printf "\n\n========================================\nPLIK: {}\n========================================\n" \; -exec cat {} \; > kod_projektu.txt
