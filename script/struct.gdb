define getaddress
	set $arg0 = &*($arg1 *)$arg2
end

define getsymbol
	getaddress $symbol RuleSymbol $arg0
	if $symbol
		printf "0x%x=>%s", $symbol, $symbol->SymbolName
	end
end

define getproduction
	getaddress $production RuleSymbol $arg0
	while $production
		set $symbol = $production
		while $symbol
			getsymbol $symbol
			set $symbol = $symbol->pNextSymbol
		end
		set $production = $production->pNextProduction
		if $production
			echo |
		end
	end
end

define getrule
	getaddress $rule Rule $arg0
	while $rule
		printf "0x%x=>%s", $rule, $rule->RuleName
		echo |
		set $production = $rule->pFirstProduction
		getproduction $production
		set $rule = $rule->pNextRule
		if $rule
			echo ||
		end
	end
end

define getvoidtable
	set $voidtable = &*(struct _VoidTable *)$arg0
	set $i = 0
	while $i < $voidtable->ColCount
		printf "%s", $voidtable->pTableHead[$i]
		set $i = $i + 1
	end
	echo |
	set $i = 0
	while $i < $voidtable->ColCount
		printf "%d", $voidtable->TableRows->hasVoid[$i]
		set $i = $i + 1
	end
end

define getset
	getaddress $set Set $arg0
	set $i = 0
	printf "0x%x=>%s", $set, $set->Name
	while $i < $set->nTerminalCount
		printf "%s", $set->Terminal[$i]
		set $i = $i + 1
	end
end

define getsetlist
	getaddress $setlist SetList $arg0
	set $j = 0
	while $j < $setlist->nSetCount
		getset $setlist->Sets+$j
		set $j = $j + 1
		if $j != $setlist->nSetCount
			echo |
		end
	end
end

define getselectset
	getaddress $selectset SelectSet $arg0
	set $i = 0
	printf "0x%x=>0x%x=>0x%x", $selectset, $selectset->pRule, $selectset->pProduction
	while $i < $selectset->nTerminalCount
		printf "%s", $selectset->Terminal[$i]
		set $i = $i + 1
	end
end

define getselectsetlist
	set $selectsetlist = &*(struct _SelectSetList *)$arg0
	set $j = 0
	while $j < $selectsetlist->nSetCount
		getselectset $selectsetlist->Sets+$j
		set $j = $j + 1
		if $j != $selectsetlist->nSetCount
			echo |
		end
	end
end

define getparsingtable
	set $parsingtable = &*(struct _ParsingTable *)$arg0
	set $i = 0
	while $i < $parsingtable->ColCount
		printf "%s", $parsingtable->pTableHead[$i]
		set $i = $i + 1
	end
	echo |
	set $j = 0
	while $parsingtable->TableRows[$j].pRule
		set $i = 0
		printf "0x%x", $parsingtable->TableRows[$j].pRule
		while $i < $parsingtable->ColCount
			printf "0x%x", $parsingtable->TableRows[$j].Select[$i]
			set $i = $i + 1
		end
		set $j = $j + 1
		if $parsingtable->TableRows[$j].pRule
			echo |
		end
	end
end
