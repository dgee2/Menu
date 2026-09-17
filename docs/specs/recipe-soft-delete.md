# Recipe soft-delete retention

Recipes are soft-deleted by setting `Recipe.DeletedAtUtc`. The global query filter hides those
rows from normal recipe reads, while the owner-only restore endpoint can clear the timestamp.
Ingredients and steps remain attached to the hidden recipe so an undo restores the complete recipe.

The current retention policy is to never purge soft-deleted recipes automatically. This keeps the
undo path reliable and avoids destroying recipe history while the product does not yet have a
retention or compliance requirement. A future purge job must be introduced as a separate product
decision; it should hard-delete only rows older than the agreed retention period and rely on the
existing cascade to remove dependent ingredients and steps.
