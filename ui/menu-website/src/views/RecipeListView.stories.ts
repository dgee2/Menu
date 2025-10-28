import RecipeListView from './RecipeListView.vue';
import type { Meta, StoryObj } from '@storybook/vue3-vite';

const meta = {
  title: 'Views/RecipeListView',
  component: RecipeListView,
  tags: ['autodocs'],
  args: {},
} satisfies Meta<typeof RecipeListView>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  args: {
    recipes: [
      { id: 1, name: 'Spaghetti Bolognese', description: 'Classic Italian' },
      { id: 2, name: 'Chicken Curry', description: 'Spicy and savory' },
    ],
    isLoading: false,
  },
};

export const Loading: Story = {
  args: {
    recipes: [],
    isLoading: true,
  },
};

export const Empty: Story = {
  args: {
    recipes: [],
    isLoading: false,
  },
};
