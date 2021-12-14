Feature: The cheatsRepository allows deserializing existing cheat files into the domain model.
	It furthers can serialize existing sections into a stream that is ready to be used with the
	better edizon overlay: https://github.com/proferabg/EdiZon-Overlay


Scenario: If a file with no contents is deserialized, it at least provides a empty section for uncategorized cheats
	When I deserialize a file with the content
	"""
	"""
	Then exists 1 sections
	And the section "UnCategorized" contains 0 cheats
	
Scenario: Cheats without a prior section are appended to the UnCategorized section
	When I deserialize a file with the content	
	"""
	
	[Infinite Health]
	580f0000 02ca1a78
	580f1000 00000080
	780f0000 00000848
	610f0000 00000000 00000078


	[Max Hearts]
	580f0000 02c9fd70
	580f1000 00000038
	780f0000 000002a8
	640f0000 00000000 42f00000


	[Infinite Powers]
	580f0000 02ca1a78
	580f1000 00000080
	780f0000 00001CC8
	610f0000 00000000
	00000063 780f0000
	00000004 610f0000
	00000000 00000063
	780f0000 00000004
	610f0000 00000000 00000063
	
	"""
	Then exists 1 sections
	And the section "UnCategorized" contains 3 cheats
	And the section "UnCategorized" contains the cheats
	| Name              | Value                                                                            |
	| [Infinite Health] | 580f0000 02ca1a78 580f1000 00000080 780f0000 00000848 610f0000 00000000 00000078 |
	| [Infinite Powers] | 580f0000 02ca1a78 580f1000 00000080 780f0000 00001CC8 610f0000 00000000 00000063 780f0000 00000004 610f0000 00000000 00000063 780f0000 00000004 610f0000 00000000 00000063 |
 
Scenario: Preceeding useless crap is removed
	When I deserialize a file with the content
	"""
	
	[Penis]
	[Yeeeah]
	
	[SomeOtherEmptyTag]
	
	[NowARealCheat]
	04000000 0238FED0 52800040
	
	"""
	Then the section "UnCategorized" contains 1 cheats
	And the section "UnCategorized" contains the cheats
	| Name            | Value                      |
	| [NowARealCheat] | 04000000 0238FED0 52800040 |
 
Scenario: If the content of a Cheat is 00000000 00000000 00000000 it is treated as a section for all following cheats
	When I deserialize a file with the content
	"""
	[-- TestSection --]
	00000000 00000000 00000000

	[1. TestSectionCheat]
	680f0000 453b8000 453b8000

	[2. TestSectionCheat]
	610f0000 00000000 00000063

	""" 
	Then the section "UnCategorized" contains 0 cheats
	And the section "TestSection" contains 2 cheats
	And the section "TestSection" contains the cheats
	| Name                  | Value                      |
	| [1. TestSectionCheat] | 680f0000 453b8000 453b8000 |
	| [2. TestSectionCheat] | 610f0000 00000000 00000063 |
 
Scenario: If a file already contains sections it can still be deserialized
	When I deserialize a file with the content
	"""
	[--SectionStart:Hearts--]
	00000000 00000000 00000000

	[Infinite Health]
	580f0000 02ca1a78
	580f1000 00000080
	780f0000 00000848
	610f0000 00000000 00000078


	[Max Hearts]
	580f0000 02c9fd70
	580f1000 00000038
	780f0000 000002a8
	640f0000 00000000 42f00000


	[--SectionEnd:Hearts--]
	00000000 00000000 00000000

	[--SectionStart:Misc--]
	00000000 00000000 00000000

	[999999 Rupees]
	580f0000 02cc5fe0
	580f1000 00000e40
	580f1000 00005b08
	780f0000 00005d90
	640f0000 00000000 000f423f


	[Current Arrow 999 Enter Equipment Menu to Refill]
	580f0000 02ca6d48
	580f1000 000447e8
	780f0000 00000020
	620f0000 00000000 000003e7


	[--SectionEnd:Misc--]
	00000000 00000000 00000000

	[Infinite Stamina]
	580f0000 02c9fd70
	580f1000 00000038
	780f0000 000002ac
	680f0000 453b8000 453b8000

	[Infinite Powers]
	580f0000 02ca1a78
	580f1000 00000080
	780f0000 00001CC8
	610f0000 00000000
	00000063 780f0000
	00000004 610f0000
	00000000 00000063
	780f0000 00000004
	610f0000 00000000 00000063
	"""
	Then the section "UnCategorized" contains 2 cheats
	And the section "Hearts" contains 2 cheats
	And the section "Misc" contains 2 cheats
	And the section "UnCategorized" contains the cheats
	| Name              | Value                                                                                                                                                                      |
	| [Infinite Powers] | 580f0000 02ca1a78 580f1000 00000080 780f0000 00001CC8 610f0000 00000000 00000063 780f0000 00000004 610f0000 00000000 00000063 780f0000 00000004 610f0000 00000000 00000063 |
 
	And the section "Hearts" contains the cheats
	| Name              | Value                                                                            |
	| [Max Hearts]      | 580f0000 02c9fd70 580f1000 00000038 780f0000 000002a8 640f0000 00000000 42f00000 |
	| [Infinite Health] | 580f0000 02ca1a78 580f1000 00000080 780f0000 00000848 610f0000 00000000 00000078 |