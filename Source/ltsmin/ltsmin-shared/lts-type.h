// Copyright (c) 2008, 2009, 2010, 2011, 2012, 2013, 2014, 2015 Formal Methods and Tools, University of Twente
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
// 
//  * Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 
//  * Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 
//  * Neither the name of the University of Twente nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

/**
\file lts-type.h
\brief This data type stores signatures of labeled transition systems.

- How long is the state vector?
- Which elements of the state vector are visible and what are their types?
- How many labels of which type are there on every edge?
- How many defined state labels are there?
- What are the types used in the LTS?
*/

/**
* opaque type lts_type_t
*/
struct lts_type_s {};
typedef struct lts_type_s *lts_type_t;

/**
* enumeration of datatype representations
*/
typedef enum {
	/**
	A direct type to deal with any type that can be represented as a 32 bit integer.
	*/
	LTStypeDirect,

	/**
	A range type is used for an integer type from a (small) range of values.
	*/
	LTStypeRange,

	/**
	A chunk type is a type with a possibly infinite domain where each value is
	serialized.

	Because it is not guaranteed that all values can be enumerated, unused values
	may be garbage collected and the order of the numbering may be changed.
	*/
	LTStypeChunk,

	/**
	An enumerated type is a finite chunk type.

	It is not allowed to remove values and it is not allowed to change the
	numbering.
	*/
	LTStypeEnum
} data_format_t;

/// Create a new empty lts type.
extern lts_type_t lts_type_create();

/// Clone an lts type
extern lts_type_t lts_type_clone(lts_type_t);

/// Create a new lts type by permuting the state vector of an existing type.
extern lts_type_t lts_type_permute(lts_type_t t, int *pi);

/// Destroy an lts type.
extern void lts_type_destroy(lts_type_t *t);

/// Print the lts type to the output stream;
extern void lts_type_printf(void* l, lts_type_t t);

/// Set state length.
extern void lts_type_set_state_length(lts_type_t  t, int length);

/// Get state length.
extern int lts_type_get_state_length(lts_type_t  t);

/// Set the name of a state slot.
extern void lts_type_set_state_name(lts_type_t  t, int idx, const char* name);

/// Get the name of a state slot.
extern char* lts_type_get_state_name(lts_type_t  t, int idx);

/// Set the type of a state slot by name.
extern void lts_type_set_state_type(lts_type_t  t, int idx, const char* name);

/// Get the type of a state slot.
extern char* lts_type_get_state_type(lts_type_t  t, int idx);

/// Set the type of a state slot by type number.
extern void lts_type_set_state_typeno(lts_type_t  t, int idx, int typeno);

/// Get the type number of a state slot.
extern int lts_type_get_state_typeno(lts_type_t  t, int idx);

extern void lts_type_set_state_label_count(lts_type_t  t, int count);
extern int lts_type_get_state_label_count(lts_type_t  t);
extern void lts_type_set_state_label_name(lts_type_t  t, int label, const char*name);
extern void lts_type_set_state_label_type(lts_type_t  t, int label, const char*name);
extern void lts_type_set_state_label_typeno(lts_type_t  t, int label, int typeno);
extern char* lts_type_get_state_label_name(lts_type_t  t, int label);
extern char* lts_type_get_state_label_type(lts_type_t  t, int label);
extern int lts_type_get_state_label_typeno(lts_type_t  t, int label);
extern int lts_type_find_state_label_prefix(lts_type_t  t, const char *prefix);
extern int lts_type_find_state_label(lts_type_t  t, const char *name);

extern void lts_type_set_edge_label_count(lts_type_t  t, int count);
extern int lts_type_get_edge_label_count(lts_type_t  t);
extern void lts_type_set_edge_label_name(lts_type_t  t, int label, const char*name);
extern void lts_type_set_edge_label_type(lts_type_t  t, int label, const char*name);
extern void lts_type_set_edge_label_typeno(lts_type_t  t, int label, int typeno);
extern char* lts_type_get_edge_label_name(lts_type_t  t, int label);
extern char* lts_type_get_edge_label_type(lts_type_t  t, int label);
extern int lts_type_get_edge_label_typeno(lts_type_t  t, int label);
extern int lts_type_find_edge_label_prefix(lts_type_t  t, const char *prefix);
extern int lts_type_find_edge_label(lts_type_t  t, const char *name);

/**
* Get the number of data types used.
*/
extern int lts_type_get_type_count(lts_type_t  t);
/**
\deprecated Use lts_type_put_type instead.
\brief Add a new type with format LTStypeChunk.
*/
extern int lts_type_add_type(lts_type_t  t, const char *name, int* is_new);
/**
Get the string representation of a type.
*/
extern char* lts_type_get_type(lts_type_t  t, int typeno);
/**
* Test if a type is defined in the give LTS type.
*/
extern int lts_type_has_type(lts_type_t  t, const char *name);
/**
* Get the representation of a type.
*/
extern data_format_t lts_type_get_format(lts_type_t  t, int typeno);

/**
* Set the representation of a type.
*/
extern void lts_type_set_format(lts_type_t  t, int typeno, data_format_t format);

/** Get the maximum of a range type.
*/
extern int lts_type_get_max(lts_type_t  t, int typeno);
/** Get the minimum of a range type.
*/
extern int lts_type_get_min(lts_type_t  t, int typeno);
/** Set the range of a range type.
*/
extern void lts_type_set_range(lts_type_t  t, int typeno, int min, int max);
/**
Add a type with a given representation to the LTS type.
*/
extern int lts_type_put_type(lts_type_t  t, const char *name, data_format_t format, int* is_new);
/**
Find a type with a certain name
*/
extern int lts_type_find_type_prefix(lts_type_t  t, const char *prefix);
/**
Find a type with a name that starts with prefix
*/
extern int lts_type_find_type(lts_type_t  t, const char *name);

/**
\brief Validate the given LTS type.

If an inconsistent type is found the program aborts.
*/
extern void lts_type_validate(lts_type_t t);
