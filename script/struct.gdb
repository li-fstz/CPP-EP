define getaddress
	set $arg0 = &*($arg1 *)$arg2
end

define getsymbol
	getaddress $symbol Symbol $arg0
	if $symbol
		printf "0x%x=>%s", $symbol, ((struct Symbol *)$symbol->value)->symbolName
	end
end

define getproduction
	getaddress $production Production $arg0
	while $production
		set $symbol = ((struct Production *)$production->value)->symbolHead
		while $symbol
			getsymbol $symbol
			set $symbol = $symbol->next
		end
		set $production = $production->next
		if $production
			echo |production|
		end
	end
end

define getrule
	getaddress $rule Rule $arg0
	while $rule
		printf "0x%x=>%s", $rule, ((struct Rule *)$rule->value)->ruleName
		echo |production|
		set $production = ((struct Rule *)$rule->value)->productionHead
		getproduction $production
		set $rule = $rule->next
		if $rule
			echo |rule|
		end
	end
end

define getvoidtable
	getaddress $voidtable VoidTable $arg0
	set $i = 0
	while $i < $voidtable->colCount
		printf "%s", $voidtable->tableHead[$i]
		set $i = $i + 1
	end
	echo |voidtable|
	set $i = 0
	while $i < $voidtable->colCount
		printf "%d", ((struct VoidTableRow *)$voidtable->tableRows)[0]->hasVoid[$i]
		set $i = $i + 1
	end
end

define getset
	getaddress $set Set $arg0
	set $i = 0
	printf "0x%x=>%s", $set, $set->key
	while $i < $set->terminalCount
		printf "%s", $set->terminals[$i]
		set $i = $i + 1
	end
end

define getsetlist
	getaddress $setlist SetList $arg0
	set $j = 0
	while $j < $setlist->setCount
		getset $setlist->sets+$j
		set $j = $j + 1
		if $j != $setlist->setCount
			echo |setlist|
		end
	end
end

define getselectset
	getaddress $selectset Set $arg0
	set $i = 0
	printf "0x%x=>0x%x=>0x%x", $selectset, ((struct SelectSetKey *)$selectset->key)->rule, ((struct Production *)((struct SelectSetKey *)$selectset->key)->production->value)->symbolHead
	while $i < $selectset->terminalCount
		printf "%s", $selectset->terminals[$i]
		set $i = $i + 1
	end
end

define getselectsetlist
	getaddress $selectsetlist SetList $arg0
	set $j = 0
	while $j < $selectsetlist->setCount
		getselectset $selectsetlist->sets+$j
		set $j = $j + 1
		if $j != $selectsetlist->setCount
			echo |selectsetlist|
		end
	end
end

define getparsingtable
	getaddress $parsingtable ParsingTable $arg0
	set $i = 0
	while $i < $parsingtable->colCount
		printf "%s", $parsingtable->tableHead[$i]
		set $i = $i + 1
	end
	echo |parsingtable|
	set $j = 0
	while $j < $parsingtable->rowCount
		set $i = 0
		printf "0x%x", ((struct ParsingTableRow *)$parsingtable->tableRows)[$j].rule
		while $i < $parsingtable->colCount
			set $p = ((struct ParsingTableRow *)$parsingtable->tableRows)[$j].productions[$i]
			if $p
				printf "0x%x", ((struct Production *)$p->value)->symbolHead
			else
				echo null
			end
			set $i = $i + 1
		end
		set $j = $j + 1
		if $j < $parsingtable->rowCount
			echo |parsingtable|
		end
	end
end