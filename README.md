# SXFMParser
SXFM is for Simple XML Feature Model format.

The major advantage of this format is that one can create feature models using a simple text editor. As a consequence, SXFM models can be typed in pretty quickly. For instance, we created an SXFM file for a feature model containing 287 features in about 10-15 minutes without a single mouse click.

The SXFM format also refrains from using tags for describing features thus keeping the size of typed XML files small.

Syntax
<feature_model name="My feature model">     <-- feature model start tag and name attribute (mandatory)
  <meta>                                                                  <-- Optional
  <data name="description">Model description</data>                 <-- Optional
  <data name="creator">Model's creator</data>                       <-- Optional
  <data name="email">Model creator's email</data>                   <-- Optional
  <data name="date">Model creation date</data>                      <-- Optional
  <data name="department">Model creator's department</data>         <-- Optional
  <data name="organization">Model creator's organization</data>     <-- Optional
  <data name="address">Model creator's address</data>               <-- Optional
  <data name="phone">Model creator's phone</data>                   <-- Optional
  <data name="website">Model creator's website</data>               <-- Optional
  <data name="reference">Model's related publication</data>         <-- Optional
  </meta>
  <feature_tree>                <-- feature tree start tag
    :r root (root_id)                 <-- root feature named 'root' with unique ID 'root_id'   						
      :o opt1 (id_opt1)               <-- an optional feature named opt1 with unique id id_opt1
      :o opt2 (id_opt2)               <-- an optional feature named opt2, child of opt1 with unique id id_opt2
      :m man1                         <-- an mandatory feature named man1 with unique id id_man1
        :g [1,*]                      <-- an inclusive-OR feature group with cardinality [1..*] ([1..3] also allowed)
          : a (id_a)                  <-- a grouped feature name a with ID id_a
          : b (id_b)                  <-- a grouped feature name b with ID id_b
            :o opt3 (id_opt3)         <-- an optional feature opt3 child of b with unique id id_opt3
          : c (id_c)                  <-- a grouped feature name c with ID id_c
        :g [1,1]                      <-- an exclusive-OR feature group with cardinality [1..1]
          : d (id_d)                  <-- a grouped feature name d with ID id_d
          : e (id_e)                  <-- a grouped feature name e with ID id_e
            :g [2,3]                      <-- a feature group with cardinality [2..3] children of feature e
              : f (id_f)                  <-- a grouped feature name f with ID id_f
              : g (id_g)                  <-- a grouped feature name g with ID id_g
              : h (id_h)                  <-- a grouped feature name h with ID id_h
  </feature_tree>               <-- feature tree end tag (mandatory)
  <constraints>                 <-- extra constraints start tag (mandatory)
    c1: ~id_a or id_opt2        <-- extra constraint named c1: id_a implies id_opt2 (must be a CNF clause)
    c2: ~id_c or ~id_e          <-- extra constraint named c2: id_c excludes id_e (must be a CNF clause)
  </constraints>                <-- extra constraint end tag (mandatory)
</feature_model>                <-- feature model end tag  (mandatory)
Important:
Tags <data> are optional but encouraged
Feature tree must have at least one feature, i.e., the root node
Extra constraints are optional
Each extra constraint must be a CNF clause (any arity)
Use feature IDs (as opposed to names) to write extra constraints
Don not use parantheses to write extra constraints
The levels in the feature tree are distinguished by tabulations (\t). So the file exemplified above has hidden tabulations like this:
...
:r root (root_id)
\t:o opt1 (id_opt1)
\t:o opt2 (id_opt2)
\t:m man1        
\t\t:g [1,*]
\t\t\t: a (id_a)
\t\t\t: b (id_b)
\t\t\t\t:o opt3 (id_opt3)
\t\t\t: c (id_c)
...
