import { describe, expect, it } from 'vitest';
import { mount, type VueWrapper } from '@vue/test-utils';
import { Quasar } from 'quasar';
import { nextTick } from 'vue';
import StepRowEditor from './step-row-editor.vue';

const mountRow = (props: Record<string, unknown> = {}) =>
  mount(StepRowEditor, {
    props: { canMoveUp: false, canMoveDown: false, ...props },
    global: { plugins: [Quasar] },
    attachTo: document.body,
  });

const field = (wrapper: VueWrapper, label: string) => {
  const found = wrapper
    .findAll('.q-field')
    .find((candidate) => candidate.find('.q-field__label').text() === label);

  if (!found) throw new Error(`No field labelled "${label}"`);

  return found;
};

/** Runs the field's own Quasar rules and returns the messages that failed. */
const validate = async (wrapper: VueWrapper, label: string) => {
  const input = field(wrapper, label).findComponent({ name: 'QInput' });
  await (input.vm as unknown as { validate: () => boolean | Promise<boolean> }).validate();
  await nextTick();

  return field(wrapper, label).find('.q-field__messages').text();
};

describe('step-row-editor', () => {
  it('renders every editable field', () => {
    const wrapper = mountRow();

    const labels = wrapper.findAll('.q-field__label').map((label) => label.text());
    expect(labels).toEqual(['Title', 'Instructions', 'Duration (minutes)']);
  });

  it('emits the edited values through v-model', async () => {
    const wrapper = mountRow();

    await field(wrapper, 'Instructions').find<HTMLTextAreaElement>('textarea').setValue('Mix well');
    await field(wrapper, 'Duration (minutes)').find<HTMLInputElement>('input').setValue('5');

    expect(wrapper.emitted('update:instructionText')?.at(-1)).toEqual(['Mix well']);
    expect(wrapper.emitted('update:durationMinutes')?.at(-1)).toEqual([5]);
  });

  it('clears the duration back to null when emptied', async () => {
    const wrapper = mountRow({ durationMinutes: 5 });

    await field(wrapper, 'Duration (minutes)').find<HTMLInputElement>('input').setValue('');

    expect(wrapper.emitted('update:durationMinutes')?.at(-1)).toEqual([null]);
  });

  it('treats an untouched row as valid', async () => {
    const wrapper = mountRow();

    expect(await validate(wrapper, 'Instructions')).not.toContain('required');
  });

  it('requires the instructions once the title has been filled in', async () => {
    const wrapper = mountRow({ title: 'Preheat' });

    expect(await validate(wrapper, 'Instructions')).toContain('Instructions are required');
  });

  it('requires the instructions once a duration has been given', async () => {
    const wrapper = mountRow({ durationMinutes: 10 });

    expect(await validate(wrapper, 'Instructions')).toContain('Instructions are required');
  });

  it('rejects a zero duration', async () => {
    const wrapper = mountRow({ instructionText: 'Mix well', durationMinutes: 0 });

    expect(await validate(wrapper, 'Duration (minutes)')).toContain('Must be greater than 0');
  });

  it('rejects a fractional duration', async () => {
    const wrapper = mountRow({ instructionText: 'Mix well', durationMinutes: 1.5 });

    expect(await validate(wrapper, 'Duration (minutes)')).toContain('Must be a whole number');
  });

  it('disables the move buttons at the ends of the list', () => {
    const wrapper = mountRow({ canMoveUp: true, canMoveDown: false });

    expect(wrapper.find('[aria-label="Move step up"]').attributes('disabled')).toBeUndefined();
    expect(wrapper.find('[aria-label="Move step down"]').attributes('disabled')).toBeDefined();
  });

  it('emits move and remove events', async () => {
    const wrapper = mountRow({ canMoveUp: true, canMoveDown: true });

    await wrapper.find('[aria-label="Move step up"]').trigger('click');
    await wrapper.find('[aria-label="Move step down"]').trigger('click');
    await wrapper.find('[aria-label="Remove step"]').trigger('click');

    expect(wrapper.emitted('moveUp')).toHaveLength(1);
    expect(wrapper.emitted('moveDown')).toHaveLength(1);
    expect(wrapper.emitted('remove')).toHaveLength(1);
  });
});
