#light

// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// Note that script files will not be part of the project build.

#load "Utilities.fs"
#load "Settings.fs"
#load "DjangoContext.fs"
#load "OutputHandling.fs"

open OutputHandling

/// >>> list(smart_split(r'This is "a person\'s" test.'))
/// [u'This', u'is', u'"a person\\\'s"', u'test.']
/// >>> list(smart_split(r"Another 'person\'s' test.")) 
/// [u'Another', u"'person's'", u'test.']
/// >>> list(smart_split(r'A "\"funky\" style" test.')) 
/// [u'A', u'""funky" style"', u'test.']

smart_split "This is \"a person's\" test."
smart_split "Another 'person\'s' test."
smart_split "A \"\\\"funky\\\" style\" test."

split_token_contents "This is _(\"a person's\") test."
split_token_contents "Another '_(person\'s)' test."
split_token_contents "A _(\"\\\"funky\\\" style\" test.)"